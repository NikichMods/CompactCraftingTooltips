// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections;
using System.Reflection;

namespace CompactCraftingTooltips
{
    internal static class GameApi
    {
        private const BindingFlags PublicInstance =
            BindingFlags.Instance | BindingFlags.Public;

        private const BindingFlags PublicStatic =
            BindingFlags.Static | BindingFlags.Public;

        internal static Type ItemDefinitionType;

        private static Type _itemType;
        private static Type _gameBalanceType;
        private static Type _bubbleTextType;
        private static Type _gjlType;
        private static Type _gameSettingsType;

        private static FieldInfo _gameBalanceMe;
        private static FieldInfo _idField;
        private static FieldInfo _bubbleTextField;
        private static MethodInfo _getItemCraftsIn;
        private static MethodInfo _localize;
        private static MethodInfo _tooltipMethod;
        private static MethodInfo _getCurrentLanguage;

        internal static Guid? TryGetAssemblyCSharpMvid()
        {
            try
            {
                Assembly assembly = FindLoadedAssembly("Assembly-CSharp");
                return assembly == null
                    ? (Guid?)null
                    : assembly.ManifestModule.ModuleVersionId;
            }
            catch
            {
                return null;
            }
        }

        // Resolve the exact host contract once before Harmony installation.
        internal static void Bind()
        {
            Assembly gameAssembly =
                RequireLoadedAssembly("Assembly-CSharp");

            Assembly firstpassAssembly =
                RequireLoadedAssembly("Assembly-CSharp-firstpass");

            ItemDefinitionType =
                RequireType(gameAssembly, "ItemDefinition");
            _itemType =
                RequireType(gameAssembly, "Item");
            _gameBalanceType =
                RequireType(gameAssembly, "GameBalance");
            _bubbleTextType =
                RequireType(gameAssembly, "BubbleWidgetTextData");
            _gameSettingsType =
                RequireType(gameAssembly, "GameSettings");
            Type balanceBaseObjectType =
                RequireType(gameAssembly, "BalanceBaseObject");
            _gjlType =
                RequireType(firstpassAssembly, "GJL");

            _gameBalanceMe = RequireField(
                _gameBalanceType,
                "me",
                PublicStatic,
                _gameBalanceType);

            _idField = RequireField(
                balanceBaseObjectType,
                "id",
                PublicInstance,
                typeof(string));

            _bubbleTextField = RequireField(
                _bubbleTextType,
                "text",
                PublicInstance,
                typeof(string));

            _getItemCraftsIn = RequireMethod(
                _gameBalanceType,
                "GetItemCraftsIn",
                PublicInstance,
                new[] { typeof(string) },
                typeof(IList));

            _getCurrentLanguage = RequireMethod(
                _gameSettingsType,
                "GetCurrentLanguage",
                PublicStatic,
                Type.EmptyTypes,
                typeof(string));

            _localize = RequireMethod(
                _gjlType,
                "L",
                PublicStatic,
                new[] { typeof(string) },
                typeof(string));

            _tooltipMethod = RequireMethod(
                ItemDefinitionType,
                "GetTooltipData",
                PublicInstance,
                new[] { _itemType, typeof(bool) },
                typeof(IList));
        }

        internal static MethodInfo GetFullTooltipMethod()
        {
            return _tooltipMethod;
        }

        internal static string GetId(object value)
        {
            return value == null
                ? null
                : _idField.GetValue(value) as string;
        }

        internal static IList GetItemCraftsIn(string itemId)
        {
            object balance =
                _gameBalanceMe.GetValue(null);

            return balance == null
                ? null
                : _getItemCraftsIn.Invoke(
                    balance,
                    new object[] { itemId }) as IList;
        }

        internal static string GetCurrentLanguage()
        {
            return _getCurrentLanguage.Invoke(
                null,
                null) as string ?? string.Empty;
        }

        internal static string Localize(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key ?? string.Empty;

            return _localize.Invoke(
                null,
                new object[] { key }) as string ?? key;
        }

        internal static string GetNativeListSeparator()
        {
            string localized = Localize(",");
            return string.IsNullOrEmpty(localized) ? ", " : localized;
        }

        internal static bool TryGetBubbleText(object row, out string text)
        {
            if (row == null || !_bubbleTextType.IsInstanceOfType(row))
            {
                text = null;
                return false;
            }

            text = _bubbleTextField.GetValue(row) as string;
            return true;
        }

        internal static void SetBubbleText(object row, string text)
        {
            _bubbleTextField.SetValue(row, text);
        }

        private static Assembly FindLoadedAssembly(string simpleName)
        {
            Assembly found = null;
            Assembly[] assemblies =
                AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                AssemblyName name = assemblies[i].GetName();

                if (!string.Equals(
                    name.Name,
                    simpleName,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                if (found != null)
                    throw new AmbiguousMatchException(
                        "Multiple loaded assemblies named " +
                        simpleName + ".");

                found = assemblies[i];
            }

            return found;
        }

        private static Assembly RequireLoadedAssembly(string simpleName)
        {
            Assembly assembly = FindLoadedAssembly(simpleName);

            if (assembly == null)
                throw new TypeLoadException(
                    "Required assembly not loaded: " + simpleName);

            return assembly;
        }

        private static Type RequireType(
            Assembly assembly,
            string name)
        {
            Type type = assembly.GetType(
                name,
                false,
                false);

            if (type == null)
                throw new TypeLoadException(
                    assembly.GetName().Name + ":" + name);

            return type;
        }

        private static FieldInfo RequireField(
            Type type,
            string name,
            BindingFlags flags,
            Type expectedType)
        {
            FieldInfo field = type.GetField(name, flags);

            if (field == null ||
                field.FieldType != expectedType)
            {
                throw new MissingFieldException(
                    type.FullName,
                    name);
            }

            return field;
        }

        private static MethodInfo RequireMethod(
            Type type,
            string name,
            BindingFlags flags,
            Type[] args,
            Type expectedReturnContract)
        {
            MethodInfo method = type.GetMethod(
                name,
                flags,
                null,
                args,
                null);

            if (method == null ||
                !ReturnTypeMatches(
                    method.ReturnType,
                    expectedReturnContract))
            {
                throw new MissingMethodException(
                    type.FullName,
                    name);
            }

            return method;
        }

        private static bool ReturnTypeMatches(
            Type actual,
            Type expectedContract)
        {
            if (expectedContract == typeof(IList))
                return typeof(IList).IsAssignableFrom(actual);

            return actual == expectedContract;
        }
    }
}
