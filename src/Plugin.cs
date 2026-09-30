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
        public const string PluginVersion = "0.1.2";

        internal static ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            Guid? hostMvid = GameApi.TryGetAssemblyCSharpMvid();
            string stage = "contract-bind";

            try
            {
                GameApi.Bind();

                stage = "patch-install";

                MethodInfo postfix = typeof(RuntimePatch).GetMethod(
                    nameof(RuntimePatch.GetTooltipDataPostfix),
                    BindingFlags.Static | BindingFlags.Public);

                if (postfix == null)
                    throw new MissingMethodException(
                        "RuntimePatch.GetTooltipDataPostfix");

                _harmony = new Harmony(PluginGuid);
                _harmony.Patch(
                    GameApi.GetFullTooltipMethod(),
                    postfix: new HarmonyMethod(postfix));

                string host = RuntimeCompatibility.DescribeHost(hostMvid);

                if (RuntimeCompatibility.Classify(hostMvid) ==
                    HostIdentityStatus.Verified1407)
                {
                    Log.LogInfo(
                        PluginName + " " + PluginVersion +
                        " active; host=" + host +
                        "; contract=ok.");
                }
                else
                {
                    Log.LogWarning(
                        PluginName + " " + PluginVersion +
                        " active best-effort; host=" + host +
                        "; contract=ok.");
                }
            }
            catch (Exception ex)
            {
                TryRollbackPatch();

                Log.LogError(
                    PluginName + " " + PluginVersion +
                    " disabled at startup; host=" +
                    RuntimeCompatibility.DescribeHost(hostMvid) +
                    "; stage=" + stage +
                    "; fallback=vanilla; action=not-patched; reason=" +
                    ex.GetType().Name + ": " + ex.Message);
            }
        }

        private void OnDestroy()
        {
            if (_harmony != null)
                _harmony.UnpatchSelf();
        }

        private void TryRollbackPatch()
        {
            if (_harmony == null)
                return;

            try
            {
                _harmony.UnpatchSelf();
            }
            catch (Exception ex)
            {
                Log.LogError(
                    PluginName + " " + PluginVersion +
                    " startup rollback failed; action=unpatch-failed; reason=" +
                    ex.GetType().Name + ": " + ex.Message);
            }
            finally
            {
                _harmony = null;
            }
        }
    }

    internal static class RuntimePatch
    {
        private static readonly SessionCircuitBreaker CircuitBreaker =
            new SessionCircuitBreaker();

        public static void GetTooltipDataPostfix(
            object __instance,
            bool full_detail,
            IList __result)
        {
            if (CircuitBreaker.IsDisabled ||
                !full_detail ||
                __instance == null ||
                __result == null)
            {
                return;
            }

            try
            {
                CraftingTooltipCompactor.TryCompact(__instance, __result);
            }
            catch (Exception ex)
            {
                if (!CircuitBreaker.DisableAndShouldReport())
                    return;

                Plugin.Log.LogError(
                    Plugin.PluginName + " " + Plugin.PluginVersion +
                    " runtime failure; feature=crafting-location-compaction" +
                    "; fallback=vanilla; action=disabled-for-session; reason=" +
                    ex.GetType().Name + ": " + ex.Message);
            }
        }
    }
}
