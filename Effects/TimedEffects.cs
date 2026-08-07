using StardewValley;

namespace StardewChaos
{
    public static class TimedEffects
    {
        public static void SkipTwoHours()
        {
            int newTime = Game1.timeOfDay + 200;
            while (newTime >= 2400) newTime -= 2400;
            Game1.timeOfDay = newTime;
        }

        public static void RewindTwoHours()
        {
            int newTime = Game1.timeOfDay - 200;
            while (newTime < 600) newTime += 2400;
            if (newTime >= 2400) newTime -= 2400;
            Game1.timeOfDay = newTime;
        }
    }
}