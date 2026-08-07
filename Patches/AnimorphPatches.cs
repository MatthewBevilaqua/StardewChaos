using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewChaos.Patches
{
    public static class AnimorphPatches
    {
        public static void Apply(Harmony harmony)
        {
            var drawMethod = AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new System.Type[] { typeof(SpriteBatch) });
            if (drawMethod != null)
            {
                harmony.Patch(drawMethod,
                    prefix: new HarmonyMethod(typeof(AnimorphPatches), nameof(FarmerDraw_Prefix)));
            }
        }

        static bool FarmerDraw_Prefix()
        {
            if (AnimorphManager.IsActive) return false;
            return true;
        }
    }
}