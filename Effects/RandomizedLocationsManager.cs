using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChaos
{
    public static class RandomizedLocationsManager
    {
        private static bool _active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private static bool _skipNextWarp;

        private class WarpDest
        {
            public string LocationName;
            public int TileX;
            public int TileY;
        }

        private static readonly WarpDest[] Destinations =
        {
            new WarpDest { LocationName = "Farm", TileX = 64, TileY = 15 },
            new WarpDest { LocationName = "BusStop", TileX = 8, TileY = 10 },
            new WarpDest { LocationName = "Town", TileX = 38, TileY = 40 },
            new WarpDest { LocationName = "Beach", TileX = 20, TileY = 4 },
            new WarpDest { LocationName = "Forest", TileX = 10, TileY = 10 },
            new WarpDest { LocationName = "Mountain", TileX = 10, TileY = 8 },
            new WarpDest { LocationName = "Railroad", TileX = 10, TileY = 30 },
            new WarpDest { LocationName = "Backwoods", TileX = 0, TileY = 10 },
        };

        public static bool IsActive => _active;

        public static void Start()
        {
            _ticksTotal = 3 * 60 * 60;
            _ticksRemaining = _ticksTotal;
            _active = true;
            _skipNextWarp = false;
            ModEntry.Instance?.Monitor?.Log("Randomized Locations activated for 3 minutes.", LogLevel.Info);
            try { Game1.addHUDMessage(new HUDMessage("Exits are randomized!", HUDMessage.error_type)); } catch { }
        }

        public static void Stop()
        {
            _active = false;
            ModEntry.Instance?.Monitor?.Log("Randomized Locations deactivated.", LogLevel.Info);
        }

        public static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!_active) return;
            if (e.NewLocation == null) return;

            if (_skipNextWarp)
            {
                _skipNextWarp = false;
                return;
            }

            string newLocName = e.NewLocation.Name;

            bool isMainLocation = false;
            foreach (var d in Destinations)
            {
                if (d.LocationName == newLocName) { isMainLocation = true; break; }
            }
            if (!isMainLocation) return;

            var dest = Destinations[Game1.random.Next(Destinations.Length)];
            int attempts = 0;
            while (dest.LocationName == newLocName && attempts < 5)
            {
                dest = Destinations[Game1.random.Next(Destinations.Length)];
                attempts++;
            }
            if (dest.LocationName == newLocName) return;

            int offsetX = Game1.random.Next(3, 8);
            int offsetY = Game1.random.Next(3, 8);

            ModEntry.Instance?.Monitor?.Log($"Randomized Locations: arrived at {newLocName}, re-warping to {dest.LocationName} (offset {offsetX},{offsetY}).", LogLevel.Info);
            _skipNextWarp = true;
            try
            {
                Game1.warpFarmer(dest.LocationName, dest.TileX + offsetX, dest.TileY + offsetY, false);
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Monitor?.Log($"Randomized Locations warp failed: {ex.Message}", LogLevel.Warn);
                _skipNextWarp = false;
            }
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;

            _ticksRemaining--;
            if (_ticksRemaining <= 0)
            {
                Stop();
                return;
            }
        }

        public static float Progress => _active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
    }
}