using HarmonyLib;
using StardewValley;

namespace StardewChaos.Patches
{
    public static class MovementPatches
    {
        public static bool IceModeActive = false;
        public static bool InvertControlsActive = false;
        public static int LockedDirection = -1;

        public static void Apply(Harmony harmony)
        {
            var setMoving = AccessTools.Method(typeof(Farmer), "setMoving");
            if (setMoving != null)
            {
                harmony.Patch(setMoving,
                    prefix: new HarmonyMethod(typeof(MovementPatches), nameof(SetMoving_Prefix)));
            }

            var movePosition = AccessTools.Method(typeof(Farmer), "MovePosition");
            if (movePosition != null)
            {
                harmony.Patch(movePosition,
                    prefix: new HarmonyMethod(typeof(MovementPatches), nameof(MovePosition_Prefix)));
            }
        }

        static void SetMoving_Prefix(Farmer __instance, ref byte command)
        {
            if (InvertControlsActive)
            {
                switch (command)
                {
                    case 1: command = 4; break;
                    case 4: command = 1; break;
                    case 2: command = 8; break;
                    case 8: command = 2; break;
                    case 33: command = 36; break;
                    case 36: command = 33; break;
                    case 34: command = 40; break;
                    case 40: command = 34; break;
                }
            }

            if (IceModeActive && command != 0)
            {
                byte dirByte = (byte)(command & 0x0F);
                switch (dirByte)
                {
                    case 1: LockedDirection = 0; break;
                    case 2: LockedDirection = 1; break;
                    case 4: LockedDirection = 2; break;
                    case 8: LockedDirection = 3; break;
                }
                ScreenEffects.IceDirection = LockedDirection;
            }
        }

        static void MovePosition_Prefix(Farmer __instance)
        {
            if (IceModeActive && LockedDirection >= 0)
            {
                __instance.movementDirections.Clear();
                __instance.movementDirections.Add(LockedDirection);
            }
        }
    }
}