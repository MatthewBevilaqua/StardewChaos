using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChaos
{
    public class GrieferJesusOutThereManager
    {
        private readonly ModEntry _mod;
        private static HostileVillager _jesus;
        private static int _startGameTime = -1;
        private const int DurationGameMinutes = 180;
        private static string _lockedLocation;

        private static readonly string[] CuratedLocations =
        {
            "Farm", "BusStop", "Town", "Beach", "Forest", "Mountain", "Railroad"
        };

        public GrieferJesusOutThereManager(ModEntry mod)
        {
            _mod = mod;
        }

        public string LockedLocation => _lockedLocation;

        public void Activate()
        {
            try
            {
                var candidates = new List<string>();
                foreach (var name in CuratedLocations)
                {
                    if (Game1.getLocationFromName(name) != null)
                        candidates.Add(name);
                }
                if (candidates.Count == 0)
                {
                    _mod.Monitor.Log("Griefer Jesus out there: no valid locations found.", LogLevel.Warn);
                    return;
                }

                _lockedLocation = candidates[Game1.random.Next(candidates.Count)];
                var target = Game1.getLocationFromName(_lockedLocation);
                SpawnJesusInLocation(target);
                _startGameTime = Game1.timeOfDay;
                _mod.Monitor.Log($"Griefer Jesus is out there: locking down {_lockedLocation} for {DurationGameMinutes} game-minutes.", LogLevel.Info);
                try { Game1.addHUDMessage(new HUDMessage($"Griefer Jesus is locking down: {_lockedLocation}!", HUDMessage.error_type)); } catch { }
            }
            catch (Exception ex)
            {
                _mod.Monitor.Log($"Griefer Jesus out there activate failed: {ex.Message}", LogLevel.Error);
            }
        }

        public void Deactivate()
        {
            DespawnJesus();
            _startGameTime = -1;
            _lockedLocation = null;
            _mod.Monitor.Log("Griefer Jesus is out there: deactivated.", LogLevel.Info);
        }

        public void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (_startGameTime < 0 || _jesus == null) return;

            int elapsed = Game1.timeOfDay - _startGameTime;
            if (elapsed < 0) elapsed += 2400;

            if (elapsed >= DurationGameMinutes)
            {
                _mod.Monitor.Log("Griefer Jesus out there: time expired, despawning.", LogLevel.Info);
                Deactivate();
                _mod.DailyManager.SetDailyActive(null, "None");
            }
        }

        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (_jesus == null || _startGameTime < 0) return;

            if (_jesus.Health <= 0)
                _jesus.Health = 999999;
        }

        private void SpawnJesusInLocation(GameLocation target)
        {
            if (target == null) return;

            int attempts = 0;
            while (attempts < 50)
            {
                attempts++;
                int tx = Game1.random.Next(target.Map.GetLayer("Back").LayerWidth);
                int ty = Game1.random.Next(target.Map.GetLayer("Back").LayerHeight);
                var tile = new Vector2(tx, ty);
                if (!target.isTileOnMap(tile)) continue;
                if (target.IsTileOccupiedBy(tile)) continue;

                var pos = new Vector2(tx * 64f, ty * 64f);
                _jesus = new HostileVillager(pos, chaseSpeed: 3.2f, contactDamage: 40, health: 999999, invincible: true)
                {
                    Name = "GrieferJesus",
                    displayName = "Griefer Jesus",
                    SimpleNonVillagerNPC = true
                };

                target.addCharacter(_jesus);
                _mod.Monitor.Log($"Griefer Jesus spawned at {target.Name} ({tx},{ty}).", LogLevel.Info);
                return;
            }
            _mod.Monitor.Log($"Griefer Jesus: could not find open tile in {target.Name}.", LogLevel.Warn);
        }

        private void DespawnJesus()
        {
            if (_jesus == null) return;
            foreach (GameLocation loc in Game1.locations)
            {
                if (loc.characters.Contains(_jesus))
                {
                    loc.characters.Remove(_jesus);
                    break;
                }
            }
            _jesus = null;
        }
    }
}