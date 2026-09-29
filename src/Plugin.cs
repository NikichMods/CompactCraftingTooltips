// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CompactCraftingTooltips
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.gyk.compactcraftingtooltips";
        public const string PluginName = "Compact Crafting Tooltips";
        public const string PluginVersion = "0.1.1";

        internal static ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            try
            {
                GameApi.Bind();

                MethodInfo tooltip = GameApi.GetFullTooltipMethod();
                if (tooltip == null)
                    throw new MissingMethodException("ItemDefinition.GetTooltipData(Item,bool)");

                _harmony = new Harmony(PluginGuid);
                _harmony.Patch(
                    tooltip,
                    postfix: new HarmonyMethod(
                        typeof(RuntimePatch).GetMethod(
                            nameof(RuntimePatch.GetTooltipDataPostfix),
                            BindingFlags.Static | BindingFlags.Public)));

                Log.LogInfo(PluginName + " " + PluginVersion + " loaded.");
            }
            catch (Exception ex)
            {
                Log.LogError(PluginName + " failed to initialize: " + ex);
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
        private static bool _failureLogged;

        public static void GetTooltipDataPostfix(
            object __instance,
            bool full_detail,
            IList __result)
        {
            if (!full_detail || __instance == null || __result == null)
                return;

            try
            {
                CraftingTooltipCompactor.TryCompact(__instance, __result);
            }
            catch (Exception ex)
            {
                if (_failureLogged)
                    return;

                _failureLogged = true;
                Plugin.Log.LogError(
                    "Crafting-location compaction failed; leaving vanilla tooltip text unchanged: " +
                    ex);
            }
        }
    }
}
