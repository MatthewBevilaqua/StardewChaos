using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace StardewChaos.Patches
{
    public static class MousePatches
    {
        public static void Apply(Harmony harmony, IMonitor monitor)
        {
            var getState = AccessTools.Method(typeof(Mouse), nameof(Mouse.GetState));
            if (getState != null)
            {
                harmony.Patch(getState,
                    postfix: new HarmonyMethod(typeof(MousePatches), nameof(GetState_Postfix)));
                monitor.Log("MousePatches: patched Mouse.GetState().", LogLevel.Info);
            }
            else
            {
                monitor.Log("MousePatches: could not find Mouse.GetState().", LogLevel.Warn);
            }
        }

        static void GetState_Postfix(ref MouseState __result)
        {
            if (!MovementPatches.InvertControlsActive) return;

            var left = __result.LeftButton;
            var right = __result.RightButton;
            if (left == right) return;

            __result = new MouseState(
                __result.X,
                __result.Y,
                __result.ScrollWheelValue,
                right,
                __result.MiddleButton,
                left,
                __result.XButton1,
                __result.XButton2
            );
        }
    }
}