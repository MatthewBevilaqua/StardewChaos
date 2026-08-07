using HarmonyLib;

namespace StardewChaos.Patches
{
    public static class TimePatches
    {
        public static bool FreezeTime = false;
        public static float PriceMultiplier = 1f;
        public static int LockedTimeOfDay = -1;

        public static void Apply(Harmony harmony)
        {
            PatchTimeAdvance(harmony);
            PatchTimeOfDaySetter(harmony);
            PatchShopPrice(harmony);
            PatchLighting(harmony);
        }

        private static void PatchTimeAdvance(Harmony harmony)
        {
            var original = AccessTools.Method("StardewValley.Game1:updateGameClock");
            if (original == null) return;
            harmony.Patch(original,
                prefix: new HarmonyMethod(typeof(TimePatches), nameof(TimeAdvance_Prefix)));
        }

        static bool TimeAdvance_Prefix(ref int ___timeOfDay)
        {
            if (FreezeTime) return false;
            return true;
        }

        private static void PatchTimeOfDaySetter(Harmony harmony)
        {
            var prop = AccessTools.Property("StardewValley.Game1:timeOfDay");
            if (prop == null || prop.GetSetMethod() == null) return;
            harmony.Patch(prop.GetSetMethod(),
                postfix: new HarmonyMethod(typeof(TimePatches), nameof(TimeOfDay_Postfix)));
        }

        static void TimeOfDay_Postfix(ref int __0)
        {
            if (LockedTimeOfDay != -1)
                __0 = LockedTimeOfDay;
        }

        private static void PatchShopPrice(Harmony harmony)
        {
            var original = AccessTools.Method("StardewValley.Objects.SObject:getSellToStorePrice");
            if (original == null) return;
            harmony.Patch(original,
                postfix: new HarmonyMethod(typeof(TimePatches), nameof(SellPrice_Postfix)));
        }

        static void SellPrice_Postfix(ref int __result)
        {
            if (PriceMultiplier != 1f && __result > 0)
                __result = (int)(__result * PriceMultiplier);
        }

        private static void PatchLighting(Harmony harmony)
        {
            // Stub: lighting lock is handled via LockedTimeOfDay; no extra patch needed for v1
        }
    }
}