using HarmonyLib;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace StardewChaos.Patches
{
    public static class UpdatePatches
    {
        public static bool GameSpeedupActive = false;
        private static bool _isReentering = false;

        private static readonly MethodInfo UpdateMethod =
            typeof(StardewValley.Game1).GetMethod("Update",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new[] { typeof(GameTime) }, null);

        public static void Apply(Harmony harmony)
        {
            if (UpdateMethod != null)
            {
                harmony.Patch(UpdateMethod,
                    postfix: new HarmonyMethod(typeof(UpdatePatches), nameof(Update_Postfix)));
            }
        }

        static void Update_Postfix(object __instance, GameTime gameTime)
        {
            if (!GameSpeedupActive || UpdateMethod == null || _isReentering) return;
            try
            {
                _isReentering = true;
                UpdateMethod.Invoke(__instance, new object[] { gameTime });
            }
            catch { }
            finally
            {
                _isReentering = false;
            }
        }
    }
}