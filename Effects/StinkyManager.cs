using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Pathfinding;

namespace StardewChaos
{
    public class StinkyManager
    {
        private readonly ModEntry _mod;
        private bool _active;
        private int _tickCounter;
        private const int FleeRadius = 15;
        private const int FleeSpeed = 3;
        private const int NormalSpeed = 2;

        public StinkyManager(ModEntry mod)
        {
            _mod = mod;
        }

        public void Activate()
        {
            _active = true;
            _mod.Monitor.Log("Ew, stinky! activated.", LogLevel.Info);
        }

        public void Deactivate()
        {
            _active = false;
            ClearAllControllers();
            _mod.Monitor.Log("Ew, stinky! deactivated.", LogLevel.Info);
        }

        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (Game1.currentLocation == null) return;

            _tickCounter++;
            if (_tickCounter < 5) return;
            _tickCounter = 0;

            var player = Game1.player;
            var loc = Game1.currentLocation;
            var playerTile = player.TilePoint;

            foreach (var npc in loc.characters)
            {
                if (npc == null) continue;
                if (npc is StardewValley.Monsters.Monster) continue;
                if (npc.SimpleNonVillagerNPC) continue;
                if (npc.IsInvisible) continue;
                if (npc is StardewValley.Characters.Pet) continue;
                if (npc is StardewValley.Characters.Horse) continue;
                if (npc is StardewValley.Characters.Child) continue;

                var npcTile = npc.TilePoint;
                float dist = Math.Abs(npcTile.X - playerTile.X) + Math.Abs(npcTile.Y - playerTile.Y);

                if (dist <= FleeRadius)
                {
                    npc.speed = FleeSpeed;

                    int fleeDx = npcTile.X - playerTile.X;
                    int fleeDy = npcTile.Y - playerTile.Y;
                    if (fleeDx == 0 && fleeDy == 0) { fleeDx = Game1.random.Next(-1, 2); fleeDy = Game1.random.Next(-1, 2); }

                    int fleeRange = FleeRadius + 4;
                    int targetX = Math.Clamp(npcTile.X + (fleeDx == 0 ? Game1.random.Next(-4, 5) : (int)(fleeDx * fleeRange / Math.Max(1, Math.Abs(fleeDx)))), 0, loc.Map.GetLayer("Back").LayerWidth - 1);
                    int targetY = Math.Clamp(npcTile.Y + (fleeDy == 0 ? Game1.random.Next(-4, 5) : (int)(fleeDy * fleeRange / Math.Max(1, Math.Abs(fleeDy)))), 0, loc.Map.GetLayer("Back").LayerHeight - 1);

                    try
                    {
                        var target = new Point(targetX, targetY);
                        if (loc.isTileOnMap(new Vector2(target.X, target.Y))
                            && loc.isTilePassable(new xTile.Dimensions.Location(target.X, target.Y), Game1.viewport))
                        {
                            npc.controller = new PathFindController(npc, loc, target, npc.FacingDirection);
                        }
                    }
                    catch { }
                }
                else
                {
                    if (npc.controller != null)
                    {
                        try { npc.controller = null; } catch { }
                    }
                    npc.speed = NormalSpeed;
                }
            }
        }

        private void ClearAllControllers()
        {
            foreach (GameLocation loc in Game1.locations)
            {
                if (loc?.characters == null) continue;
                foreach (var npc in loc.characters)
                {
                    try
                    {
                        if (npc != null)
                        {
                            npc.controller = null;
                            npc.speed = NormalSpeed;
                        }
                    }
                    catch { }
                }
            }
        }
    }
}