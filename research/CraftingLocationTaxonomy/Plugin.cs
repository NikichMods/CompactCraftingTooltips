// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CompactCraftingTooltips.Research
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.gyk.compactcraftingtooltips.research";
        public const string PluginName = "Compact Crafting Tooltips Research";
        public const string PluginVersion = "0.0.0-research";

        internal static ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            try
            {
                GameApi.Bind();
                _harmony = new Harmony(PluginGuid);

                MethodInfo started = GameApi.MainGameType.GetMethod(
                    "OnGameStartedPlaying",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (started == null)
                    throw new MissingMethodException("MainGame.OnGameStartedPlaying");

                _harmony.Patch(
                    started,
                    postfix: new HarmonyMethod(
                        typeof(RuntimePatch).GetMethod(
                            nameof(RuntimePatch.OnGameStartedPlayingPostfix),
                            BindingFlags.Static | BindingFlags.Public)));

                Log.LogInfo(PluginName + " " + PluginVersion + " loaded.");
            }
            catch (Exception ex)
            {
                Log.LogError("Research probe failed to initialize: " + ex);
            }
        }

        private void OnDestroy()
        {
            if (_harmony != null)
                _harmony.UnpatchSelf();
        }
    }

    internal static class RuntimePatch
    {
        private static bool _dumped;

        public static void OnGameStartedPlayingPostfix()
        {
            if (_dumped)
                return;

            _dumped = true;

            try
            {
                TaxonomyProbe.Dump();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("CCT_RESEARCH_ERROR phase=dump error=" + ex);
            }
        }
    }

    internal static class TaxonomyProbe
    {
        private static readonly Regex NumericSuffix =
            new Regex(@"^(?<stem>.+)_(?<tier>\d+)$", RegexOptions.CultureInvariant);

        internal static void Dump()
        {
            IList items = GameApi.GetItems();
            if (items == null)
                throw new InvalidOperationException("GameBalance.items_data is not available.");

            Dictionary<string, StationSnapshot> stations =
                new Dictionary<string, StationSnapshot>(StringComparer.Ordinal);

            int itemsWithLocations = 0;
            int itemsWithMultipleLocations = 0;
            int rawLinks = 0;

            Plugin.Log.LogInfo(
                "CCT_RESEARCH_START" +
                " game_version=" + Quote(GameApi.GetGameVersion()) +
                " language=" + Quote(GameApi.GetCurrentLanguage()) +
                " items=" + items.Count);

            foreach (object item in items)
            {
                string itemId = GameApi.GetId(item);
                if (string.IsNullOrEmpty(itemId))
                    continue;

                IList craftLocations = GameApi.GetItemCraftsIn(itemId);
                if (craftLocations == null || craftLocations.Count == 0)
                    continue;

                itemsWithLocations++;
                rawLinks += craftLocations.Count;

                if (craftLocations.Count > 1)
                    itemsWithMultipleLocations++;

                List<string> orderedIds = new List<string>();
                List<string> orderedNames = new List<string>();

                foreach (object location in craftLocations)
                {
                    if (location == null)
                        continue;

                    string stationId = GameApi.GetId(location);
                    if (string.IsNullOrEmpty(stationId))
                        continue;

                    orderedIds.Add(stationId);
                    orderedNames.Add(GameApi.Localize(stationId));

                    if (!stations.ContainsKey(stationId))
                        stations.Add(stationId, GameApi.ReadStation(location));
                }

                if (craftLocations.Count > 1)
                {
                    Plugin.Log.LogInfo(
                        "CCT_ITEM" +
                        " item=" + Quote(itemId) +
                        " station_count=" + craftLocations.Count +
                        " ids=" + Quote(string.Join(" -> ", orderedIds.ToArray())) +
                        " names=" + Quote(string.Join(" -> ", orderedNames.ToArray())));
                }
            }

            foreach (StationSnapshot station in stations.Values.OrderBy(s => s.Id, StringComparer.Ordinal))
            {
                Plugin.Log.LogInfo(
                    "CCT_STATION" +
                    " id=" + Quote(station.Id) +
                    " name=" + Quote(station.LocalizedName) +
                    " craft_preset=" + Quote(station.CraftPreset) +
                    " subtype=" + Quote(station.FilterCraftSubtype) +
                    " interaction=" + Quote(station.InteractionType) +
                    " sort_n=" + station.SortN +
                    " has_craft=" + station.HasCraft +
                    " groups=" + Quote(string.Join(",", station.ObjectGroups.ToArray())));
            }

            Dictionary<string, List<StationCandidate>> candidates =
                new Dictionary<string, List<StationCandidate>>(StringComparer.Ordinal);

            foreach (StationSnapshot station in stations.Values)
            {
                Match match = NumericSuffix.Match(station.Id);
                if (!match.Success)
                    continue;

                int suffix;
                if (!int.TryParse(match.Groups["tier"].Value, out suffix))
                    continue;

                string stem = match.Groups["stem"].Value;
                List<StationCandidate> group;

                if (!candidates.TryGetValue(stem, out group))
                {
                    group = new List<StationCandidate>();
                    candidates.Add(stem, group);
                }

                group.Add(new StationCandidate
                {
                    Station = station,
                    NumericSuffix = suffix
                });
            }

            int candidateFamilyCount = 0;

            foreach (KeyValuePair<string, List<StationCandidate>> pair
                in candidates.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                if (pair.Value.Count < 2)
                    continue;

                candidateFamilyCount++;

                pair.Value.Sort((a, b) =>
                {
                    int bySuffix = a.NumericSuffix.CompareTo(b.NumericSuffix);
                    return bySuffix != 0
                        ? bySuffix
                        : string.CompareOrdinal(a.Station.Id, b.Station.Id);
                });

                string unsuffixedName = stations.ContainsKey(pair.Key)
                    ? stations[pair.Key].LocalizedName
                    : string.Empty;

                Plugin.Log.LogInfo(
                    "CCT_SUFFIX_CANDIDATE" +
                    " stem=" + Quote(pair.Key) +
                    " unsuffixed_present=" + stations.ContainsKey(pair.Key) +
                    " unsuffixed_name=" + Quote(unsuffixedName) +
                    " members=" + Quote(string.Join(
                        " | ",
                        pair.Value.Select(x =>
                            x.NumericSuffix + ":" +
                            x.Station.Id + ":" +
                            x.Station.LocalizedName).ToArray())));
            }

            Plugin.Log.LogInfo(
                "CCT_RESEARCH_DONE" +
                " items_with_locations=" + itemsWithLocations +
                " items_with_multiple_locations=" + itemsWithMultipleLocations +
                " unique_stations=" + stations.Count +
                " raw_links=" + rawLinks +
                " suffix_candidate_families=" + candidateFamilyCount +
                " save_mutation=none" +
                " ui_mutation=none");
        }

        private static string Quote(string value)
        {
            if (value == null)
                value = string.Empty;

            return "\"" +
                   value.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\r", "\\r")
                        .Replace("\n", "\\n") +
                   "\"";
        }

        private sealed class StationCandidate
        {
            internal StationSnapshot Station;
            internal int NumericSuffix;
        }
    }

    internal sealed class StationSnapshot
    {
        internal string Id;
        internal string LocalizedName;
        internal string CraftPreset;
        internal string FilterCraftSubtype;
        internal string InteractionType;
        internal int SortN;
        internal bool HasCraft;
        internal List<string> ObjectGroups = new List<string>();
    }

    internal static class GameApi
    {
        private const BindingFlags AnyInstance =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags AnyStatic =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        internal static Type MainGameType;

        private static Type _gameBalanceType;
        private static Type _objectDefinitionType;
        private static Type _gjlType;
        private static Type _gameSettingsType;
        private static Type _lazyConstsType;

        private static MemberInfo _gameBalanceMe;
        private static FieldInfo _balanceItemsField;
        private static FieldInfo _idField;
        private static MethodInfo _getItemCraftsIn;
        private static MethodInfo _localize;
        private static MethodInfo _getCurrentLanguage;
        private static PropertyInfo _gameVersionProperty;

        private static FieldInfo _craftPresetField;
        private static FieldInfo _filterCraftSubtypeField;
        private static FieldInfo _interactionTypeField;
        private static FieldInfo _sortNField;
        private static FieldInfo _hasCraftField;
        private static FieldInfo _objectGroupsField;

        internal static void Bind()
        {
            MainGameType = RequireType("MainGame");
            _gameBalanceType = RequireType("GameBalance");
            _objectDefinitionType = RequireType("ObjectDefinition");
            _gjlType = RequireType("GJL");
            _gameSettingsType = RequireType("GameSettings");
            _lazyConstsType = RequireType("LazyConsts");

            _gameBalanceMe = RequireMember(_gameBalanceType, "me", AnyStatic);
            _balanceItemsField = RequireField(_gameBalanceType, "items_data");
            _idField = RequireField(RequireType("BalanceBaseObject"), "id");
            _getItemCraftsIn = RequireMethod(
                _gameBalanceType,
                "GetItemCraftsIn",
                new[] { typeof(string) });

            _craftPresetField = RequireField(_objectDefinitionType, "craft_preset");
            _filterCraftSubtypeField = RequireField(_objectDefinitionType, "filter_craft_subtype");
            _interactionTypeField = RequireField(_objectDefinitionType, "interaction_type");
            _sortNField = RequireField(_objectDefinitionType, "sort_n");
            _hasCraftField = RequireField(_objectDefinitionType, "has_craft");
            _objectGroupsField = RequireField(_objectDefinitionType, "_object_groups");

            _localize = FindLocalizationMethod();
            _getCurrentLanguage = RequireMethod(
                _gameSettingsType,
                "GetCurrentLanguage",
                Type.EmptyTypes);

            _gameVersionProperty = _lazyConstsType.GetProperty("VERSION", AnyStatic);
        }

        internal static IList GetItems()
        {
            object balance = GetMemberValue(_gameBalanceMe, null);
            return balance == null
                ? null
                : _balanceItemsField.GetValue(balance) as IList;
        }

        internal static IList GetItemCraftsIn(string itemId)
        {
            object balance = GetMemberValue(_gameBalanceMe, null);
            return balance == null
                ? null
                : _getItemCraftsIn.Invoke(balance, new object[] { itemId }) as IList;
        }

        internal static string GetId(object value)
        {
            return value == null
                ? null
                : _idField.GetValue(value) as string;
        }

        internal static StationSnapshot ReadStation(object station)
        {
            StationSnapshot result = new StationSnapshot
            {
                Id = GetId(station) ?? string.Empty,
                CraftPreset = Convert.ToString(_craftPresetField.GetValue(station)) ?? string.Empty,
                FilterCraftSubtype = Convert.ToString(_filterCraftSubtypeField.GetValue(station)) ?? string.Empty,
                InteractionType = Convert.ToString(_interactionTypeField.GetValue(station)) ?? string.Empty,
                SortN = Convert.ToInt32(_sortNField.GetValue(station)),
                HasCraft = Convert.ToBoolean(_hasCraftField.GetValue(station))
            };

            result.LocalizedName = Localize(result.Id);

            IList groups = _objectGroupsField.GetValue(station) as IList;
            if (groups != null)
            {
                foreach (object group in groups)
                {
                    string id = group as string;
                    if (!string.IsNullOrEmpty(id))
                        result.ObjectGroups.Add(id);
                }
            }

            return result;
        }

        internal static string Localize(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key ?? string.Empty;

            try
            {
                ParameterInfo[] parameters = _localize.GetParameters();
                object[] args = new object[parameters.Length];
                args[0] = key;

                for (int i = 1; i < parameters.Length; i++)
                {
                    if (parameters[i]
                        .GetCustomAttributes(typeof(ParamArrayAttribute), false)
                        .Length > 0)
                    {
                        args[i] = Array.CreateInstance(
                            parameters[i].ParameterType.GetElementType() ?? typeof(object),
                            0);
                    }
                    else if (parameters[i].HasDefaultValue)
                    {
                        args[i] = parameters[i].DefaultValue;
                    }
                    else
                    {
                        args[i] = parameters[i].ParameterType.IsValueType
                            ? Activator.CreateInstance(parameters[i].ParameterType)
                            : null;
                    }
                }

                return _localize.Invoke(null, args) as string ?? key;
            }
            catch
            {
                return key;
            }
        }

        internal static string GetCurrentLanguage()
        {
            try
            {
                return _getCurrentLanguage.Invoke(null, null) as string ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static string GetGameVersion()
        {
            try
            {
                return _gameVersionProperty == null
                    ? string.Empty
                    : Convert.ToString(_gameVersionProperty.GetValue(null, null)) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static MethodInfo FindLocalizationMethod()
        {
            MethodInfo exact = _gjlType.GetMethod(
                "L",
                AnyStatic,
                null,
                new[] { typeof(string) },
                null);

            if (exact != null)
                return exact;

            foreach (MethodInfo method in _gjlType.GetMethods(AnyStatic))
            {
                if (method.Name != "L" || method.ReturnType != typeof(string))
                    continue;

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 0 ||
                    parameters[0].ParameterType != typeof(string))
                    continue;

                bool compatible = true;

                for (int i = 1; i < parameters.Length; i++)
                {
                    if (!parameters[i].HasDefaultValue &&
                        parameters[i]
                            .GetCustomAttributes(typeof(ParamArrayAttribute), false)
                            .Length == 0)
                    {
                        compatible = false;
                        break;
                    }
                }

                if (compatible)
                    return method;
            }

            throw new MissingMethodException("Compatible GJL.L overload not found.");
        }

        private static Type RequireType(string name)
        {
            Type type = AccessTools.TypeByName(name);
            if (type == null)
                throw new TypeLoadException(name);

            return type;
        }

        private static FieldInfo RequireField(Type type, string name)
        {
            FieldInfo field = type.GetField(name, AnyInstance | AnyStatic);
            if (field == null)
                throw new MissingFieldException(type.FullName, name);

            return field;
        }

        private static MethodInfo RequireMethod(Type type, string name, Type[] args)
        {
            MethodInfo method = type.GetMethod(
                name,
                AnyInstance | AnyStatic,
                null,
                args,
                null);

            if (method == null)
                throw new MissingMethodException(type.FullName, name);

            return method;
        }

        private static MemberInfo RequireMember(
            Type type,
            string name,
            BindingFlags flags)
        {
            FieldInfo field = type.GetField(name, flags);
            if (field != null)
                return field;

            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null)
                return property;

            throw new MissingMemberException(type.FullName, name);
        }

        private static object GetMemberValue(MemberInfo member, object instance)
        {
            FieldInfo field = member as FieldInfo;

            return field != null
                ? field.GetValue(instance)
                : ((PropertyInfo)member).GetValue(instance, null);
        }
    }
}
