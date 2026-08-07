using HarmonyLib;
using StardewValley;
using StardewValley.Monsters;
using System.Reflection;

namespace StardewChaos.Patches
{
    public static class DamagePatches
    {
        public static int DamageMultiplier = 1;

        public static void Apply(Harmony harmony)
        {
            var prefix = new HarmonyMethod(typeof(DamagePatches), nameof(TakeDamage_Prefix));
            int patchCount = 0;

            var farmerParams = new[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) };
            var baseMethod = AccessTools.Method(typeof(Monster), "takeDamage", farmerParams);
            if (baseMethod != null)
            {
                harmony.Patch(baseMethod, prefix: prefix);
                patchCount++;
            }

            foreach (var type in typeof(Monster).Assembly.GetTypes())
            {
                if (type.IsAbstract || type == typeof(Monster)) continue;
                if (!typeof(Monster).IsAssignableFrom(type)) continue;

                var overrideMethod = AccessTools.DeclaredMethod(type, "takeDamage", farmerParams);
                if (overrideMethod != null)
                {
                    harmony.Patch(overrideMethod, prefix: prefix);
                    patchCount++;
                }
            }

            ModEntry.Instance?.Monitor?.Log($"DamagePatches: patched {patchCount} takeDamage methods", StardewModdingAPI.LogLevel.Info);
        }

        static void TakeDamage_Prefix(Monster __instance, ref int damage, Farmer who)
        {
            if (damage <= 0) return;

            if (DamageMultiplier > 1 && who == Game1.player)
            {
                damage *= DamageMultiplier;
            }

            if (MitosisOnDeathEffect.Active && who == Game1.player)
            {
                MitosisOnDeathEffect.OnMonsterTakeDamage(__instance, ref damage);
            }
        }
    }
}