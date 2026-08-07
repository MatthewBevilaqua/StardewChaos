using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using Pet = StardewValley.Characters.Pet;

namespace StardewChaos
{
    public static class PetSpawnEffects
    {
        private static readonly List<NPC> _spawnedPets = new();

        public static void StopFeedingThem()
        {
            var farm = Game1.getLocationFromName("Farm") as Farm;
            if (farm == null)
            {
                ModEntry.Instance?.Monitor?.Log("Stop feeding them: Farm not found.", StardewModdingAPI.LogLevel.Warn);
                return;
            }

            string[] breeds = { "1", "2", "3", "4", "5", "6" };
            int spawned = 0;
            int attempts = 0;
            int maxAttempts = 80;

            while (spawned < 8 && attempts < maxAttempts)
            {
                attempts++;
                int tileX = Game1.random.Next(farm.Map.GetLayer("Back").LayerWidth);
                int tileY = Game1.random.Next(farm.Map.GetLayer("Back").LayerHeight);
                var tile = new Vector2(tileX, tileY);

                if (!farm.isTileOnMap(tile)) continue;
                if (farm.IsTileOccupiedBy(tile)) continue;
                if (!farm.isTilePassable(new xTile.Dimensions.Location(tileX, tileY), Game1.viewport)) continue;

                var pos = new Vector2(tileX * 64f, tileY * 64f);
                try
                {
                    var cat = new Pet
                    {
                        Name = "ChaosCat_" + spawned,
                        displayName = "ChaosCat_" + spawned,
                        SimpleNonVillagerNPC = true
                    };
                    cat.Position = pos;
                    cat.petType.Value = "Cat";
                    cat.whichBreed.Value = breeds[Game1.random.Next(breeds.Length)];
                    cat.reloadBreedSprite();
                    farm.addCharacter(cat);
                    _spawnedPets.Add(cat);
                    spawned++;
                }
                catch (Exception ex)
                {
                    ModEntry.Instance?.Monitor?.Log($"Stop feeding them: Failed to spawn cat: {ex.Message}", StardewModdingAPI.LogLevel.Warn);
                }
            }

            try { ModEntry.Instance?.Monitor?.Log($"Stop feeding them: spawned {spawned} cats.", StardewModdingAPI.LogLevel.Info); } catch { }
            Game1.playSound("dirtyHit");
        }

        public static void DespawnAll()
        {
            if (_spawnedPets.Count == 0) return;
            foreach (var pet in _spawnedPets)
            {
                if (pet == null) continue;
                foreach (GameLocation loc in Game1.locations)
                {
                    if (loc.characters.Contains(pet))
                    {
                        loc.characters.Remove(pet);
                        break;
                    }
                }
            }
            _spawnedPets.Clear();
        }
    }
}