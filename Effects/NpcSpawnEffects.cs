using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;

namespace StardewChaos
{
    public static class NpcSpawnEffects
    {
        private static readonly List<NPC> _spawnedNpcs = new();
        private const string NpcPrefix = "ChaosNpc_";

        public static void MysteriousStrangers()
        {
            var loc = Game1.currentLocation;
            if (loc == null) return;

            var player = Game1.player;
            int spawned = 0;
            int attempts = 0;
            int maxAttempts = 60;

            while (spawned < 6 && attempts < maxAttempts)
            {
                attempts++;
                int offsetX = Game1.random.Next(-6, 7);
                int offsetY = Game1.random.Next(-6, 7);
                if (offsetX == 0 && offsetY == 0) continue;

                int tileX = player.TilePoint.X + offsetX;
                int tileY = player.TilePoint.Y + offsetY;
                var tile = new Vector2(tileX, tileY);

                if (!loc.isTileOnMap(tile)) continue;
                if (loc.IsTileOccupiedBy(tile)) continue;

                var pos = new Vector2(tileX * 64f, tileY * 64f);
                var sprite = new AnimatedSprite("Characters\\Farmer\\farmer_base", 0, 16, 32);
                string name = NpcPrefix + (_spawnedNpcs.Count + spawned);

                var npc = new NPC(sprite, pos, 2, name)
                {
                    SimpleNonVillagerNPC = true,
                    displayName = ""
                };

                loc.addCharacter(npc);
                _spawnedNpcs.Add(npc);
                spawned++;
            }

            Game1.playSound("dirtyHit");
        }

        public static void DespawnAll()
        {
            if (_spawnedNpcs.Count == 0) return;

            foreach (var npc in _spawnedNpcs)
            {
                if (npc == null) continue;
                foreach (GameLocation loc in Game1.locations)
                {
                    if (loc.characters.Contains(npc))
                    {
                        loc.characters.Remove(npc);
                        break;
                    }
                }
            }
            _spawnedNpcs.Clear();
        }
    }
}