using HarmonyLib;

namespace StardewChaos.Patches
{
    public static class HudPatches
    {
        public static bool NoHudActive = false;

        public static void Apply(Harmony harmony)
        {
            var drawHUD = AccessTools.Method(typeof(StardewValley.Game1), "drawHUD");
            if (drawHUD != null)
            {
                harmony.Patch(drawHUD,
                    prefix: new HarmonyMethod(typeof(HudPatches), nameof(DrawHUD_Prefix)));
            }
        }

        static bool DrawHUD_Prefix()
        {
            return !NoHudActive;
        }
    }
}