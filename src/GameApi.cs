// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;

namespace CompactCraftingTooltips
{
    internal static class GameApi
    {
        private const BindingFlags AnyInstance =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags AnyStatic =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        internal static Type ItemDefinitionType;

        private static Type _itemType;
        private static Type _gameBalanceType;
        private static Type _bubbleTextType;
        private static Type _gjlType;

        private static MemberInfo _gameBalanceMe;
        private static FieldInfo _idField;
        private static FieldInfo _bubbleTextField;
        private static MethodInfo _getItemCraftsIn;
        private static MethodInfo _localize;
        private static MethodInfo _tooltipMethod;

        internal static void Bind()
        {
            ItemDefinitionType = RequireType("ItemDefinition");
            _itemType = RequireType("Item");
            _gameBalanceType = RequireType("GameBalance");
            _bubbleTextType = RequireType("BubbleWidgetTextData");
            _gjlType = RequireType("GJL");

            _gameBalanceMe = RequireMember(_gameBalanceType, "me", AnyStatic);
            _idField = RequireField(RequireType("BalanceBaseObject"), "id");
            _bubbleTextField = RequireField(_bubbleTextType, "text");
            _getItemCraftsIn = RequireMethod(
                _gameBalanceType,
                "GetItemCraftsIn",
                new[] { typeof(string) });

            _localize = FindLocalizationMethod();

            _tooltipMethod = ItemDefinitionType.GetMethod(
                "GetTooltipData",
                AnyInstance,
                null,
                new[] { _itemType, typeof(bool) },
                null);

            if (_tooltipMethod == null)
                throw new MissingMethodException(
                    "ItemDefinition.GetTooltipData(Item,bool)");
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
            object balance = GetMemberValue(_gameBalanceMe, null);
            return balance == null
                ? null
                : _getItemCraftsIn.Invoke(
                    balance,
                    new object[] { itemId }) as IList;
        }

        internal static string Localize(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key ?? string.Empty;

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
                        parameters[i].ParameterType.GetElementType()
                            ?? typeof(object),
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
                            .GetCustomAttributes(
                                typeof(ParamArrayAttribute),
                                false)
                            .Length == 0)
                    {
                        compatible = false;
                        break;
                    }
                }

                if (compatible)
                    return method;
            }

            throw new MissingMethodException(
                "Compatible GJL.L overload not found.");
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

        private static MethodInfo RequireMethod(
            Type type,
            string name,
            Type[] args)
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

        private static object GetMemberValue(
            MemberInfo member,
            object instance)
        {
            FieldInfo field = member as FieldInfo;
            return field != null
                ? field.GetValue(instance)
                : ((PropertyInfo)member).GetValue(instance, null);
        }
    }
}
