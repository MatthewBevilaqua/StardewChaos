using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewChaos
{
    public static class NpcEffects
    {
        private static readonly Dictionary<string, int> SavedFriendship = new();
        private static readonly List<(NPC npc, string locName, Point tile)> _partyNpcs = new();
        private static readonly List<(NPC npc, string locName, Point tile)> _townSquareNpcs = new();

        public static void StartNpcTownSquare()
        {
            _townSquareNpcs.Clear();
            var town = Game1.getLocationFromName("Town");
            if (town == null) return;

            var allNpcs = new List<(NPC npc, string locName, Point tile)>();
            foreach (GameLocation loc in Game1.locations)
            {
                var npcsCopy = loc.characters.ToList();
                foreach (NPC npc in npcsCopy)
                {
                    if (npc == null || !npc.IsVillager) continue;
                    allNpcs.Add((npc, loc.NameOrUniqueName, new Point(npc.TilePoint.X, npc.TilePoint.Y)));
                }
            }

            int x = 20, y = 20;
            foreach (var (npc, locName, origTile) in allNpcs)
            {
                if (npc.currentLocation != town)
                {
                    npc.currentLocation?.characters.Remove(npc);
                    town.addCharacter(npc);
                }
                npc.setTilePosition(new Point(x, y));
                x++;
                if (x > 40) { x = 20; y++; }
                _townSquareNpcs.Add((npc, locName, origTile));
            }

            Game1.playSound("dwop");
        }

        public static void StopNpcTownSquare()
        {
            foreach (var (npc, locName, tile) in _townSquareNpcs)
            {
                if (npc == null) continue;
                var loc = Game1.getLocationFromName(locName);
                if (loc != null)
                {
                    if (npc.currentLocation != loc)
                    {
                        npc.currentLocation?.characters.Remove(npc);
                        loc.addCharacter(npc);
                    }
                    npc.setTilePosition(tile);
                }
            }
            _townSquareNpcs.Clear();
        }

        public static void SetFriendship(int value)
        {
            SavedFriendship.Clear();
            var farmer = Game1.player;
            foreach (string name in farmer.friendshipData.Keys)
            {
                SavedFriendship[name] = farmer.friendshipData[name].Points;
                farmer.friendshipData[name].Points = value;
            }
        }

        public static void RestoreFriendship()
        {
            var farmer = Game1.player;
            foreach (var kvp in SavedFriendship)
            {
                if (farmer.friendshipData.ContainsKey(kvp.Key))
                    farmer.friendshipData[kvp.Key].Points = kvp.Value;
            }
            SavedFriendship.Clear();
        }

        public static void StartPartyTime()
        {
            _partyNpcs.Clear();
            var player = Game1.player;
            var playerLoc = Game1.currentLocation;
            if (playerLoc == null) return;

            int px = player.TilePoint.X;
            int py = player.TilePoint.Y;
            int radius = 3;

            var allNpcs = new List<(NPC npc, string locName, Point tile)>();
            foreach (GameLocation loc in Game1.locations)
            {
                var npcsCopy = loc.characters.ToList();
                foreach (NPC npc in npcsCopy)
                {
                    if (npc == null || !npc.IsVillager) continue;
                    allNpcs.Add((npc, loc.NameOrUniqueName, new Point(npc.TilePoint.X, npc.TilePoint.Y)));
                }
            }

            int placed = 0;
            foreach (var (npc, locName, origTile) in allNpcs)
            {
                int angle = placed * 36;
                int r = radius + (placed / 10);
                int tx = px + (int)(Math.Cos(angle * Math.PI / 180.0) * r);
                int ty = py + (int)(Math.Sin(angle * Math.PI / 180.0) * r);
                placed++;

                var tile = new Vector2(tx, ty);
                if (!playerLoc.isTileOnMap(tile) || playerLoc.IsTileBlockedBy(tile))
                {
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            var alt = new Vector2(px + dx, py + dy);
                            if (playerLoc.isTileOnMap(alt) && !playerLoc.IsTileBlockedBy(alt))
                            {
                                tx = (int)alt.X; ty = (int)alt.Y;
                                goto found;
                            }
                        }
                    }
                    continue;
                    found:;
                }

                if (npc.currentLocation != playerLoc)
                {
                    npc.currentLocation?.characters.Remove(npc);
                    playerLoc.addCharacter(npc);
                }
                npc.setTilePosition(new Point(tx, ty));
                _partyNpcs.Add((npc, locName, origTile));
            }

            Game1.playSound("harp");
            try { Game1.changeMusicTrack("sun", false); } catch { }
        }

        public static void StopPartyTime()
        {
            foreach (var (npc, locName, tile) in _partyNpcs)
            {
                if (npc == null) continue;
                var loc = Game1.getLocationFromName(locName);
                if (loc != null)
                {
                    if (npc.currentLocation != loc)
                    {
                        npc.currentLocation?.characters.Remove(npc);
                        loc.addCharacter(npc);
                    }
                    npc.setTilePosition(tile);
                }
            }
            _partyNpcs.Clear();
            try { Game1.changeMusicTrack("none"); } catch { }
        }
    }
}