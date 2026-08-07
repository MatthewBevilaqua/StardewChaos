using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewChaos
{
    public static class GrieferJesusEffects
    {
        private static readonly List<NPC> _spawned = new();
        private static int _despawnTimer = 0;
        private static int _despawnTotal = 0;
        private static bool _hereActive = false;
        private static int _warningTimer = 0;
        private const int WarningDuration = 180;

        public static bool IsHereActive => _hereActive;

        public static void JesusIsHere()
        {
            var loc = Game1.currentLocation;
            if (loc == null) return;
            var player = Game1.player;

            _spawned.Clear();
            _despawnTotal = 15 * 60;
            _despawnTimer = 0;
            _warningTimer = 0;
            _hereActive = true;

            var jesus = SpawnJesus(loc, player.TilePoint);
            if (jesus != null) _spawned.Add(jesus);

            for (int i = 0; i < 3; i++)
            {
                var disciple = SpawnDisciple(loc, player.TilePoint);
                if (disciple != null) _spawned.Add(disciple);
            }

            try { Game1.playSound("shadowpebble"); } catch { Game1.playSound("dirtyHit"); }
            ModEntry.Instance?.Monitor?.Log($"Griefer Jesus is here: spawned {_spawned.Count} NPCs near player (warning active).", StardewModdingAPI.LogLevel.Info);
        }

        public static void StopJesusIsHere()
        {
            DespawnAll();
            _hereActive = false;
            _despawnTimer = 0;
            _despawnTotal = 0;
        }

        public static void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!_hereActive) return;

            if (_warningTimer < WarningDuration)
            {
                _warningTimer++;
                return;
            }

            _despawnTimer++;
            if (_despawnTimer >= _despawnTotal)
            {
                StopJesusIsHere();
                return;
            }

            if (_spawned.Count == 0) return;

            foreach (var npc in _spawned)
            {
                if (npc == null) continue;
                if (npc.currentLocation != Game1.currentLocation)
                {
                    try
                    {
                        npc.currentLocation?.characters?.Remove(npc);
                        Game1.currentLocation.addCharacter(npc);
                    }
                    catch { }
                }
            }
        }

        public static void OnRendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (!_hereActive || _warningTimer >= WarningDuration) return;
            try
            {
                var b = e.SpriteBatch;
                var font = Game1.dialogueFont ?? Game1.smallFont;
                string text1 = "GREIFER JESUS IS HERE!";
                string text2 = $"{((WarningDuration - _warningTimer) / 60) + 1}...";
                var size1 = font.MeasureString(text1);
                var size2 = font.MeasureString(text2);
                float cx = Game1.graphics.GraphicsDevice.Viewport.Width / 2f;
                float cy = Game1.graphics.GraphicsDevice.Viewport.Height / 2f - 40;

                b.Begin(Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred, Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend);
                try
                {
                    b.DrawString(font, text1, new Vector2(cx - size1.X / 2f + 2, cy + 2), Microsoft.Xna.Framework.Color.Black * 0.8f);
                    b.DrawString(font, text1, new Vector2(cx - size1.X / 2f, cy), Microsoft.Xna.Framework.Color.Red);
                    b.DrawString(font, text2, new Vector2(cx - size2.X / 2f + 2, cy + size1.Y + 2), Microsoft.Xna.Framework.Color.Black * 0.8f);
                    b.DrawString(font, text2, new Vector2(cx - size2.X / 2f, cy + size1.Y), Microsoft.Xna.Framework.Color.Yellow);
                }
                finally
                {
                    b.End();
                }
            }
            catch { }
        }

        public static void DespawnAll()
        {
            foreach (var npc in _spawned)
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
            _spawned.Clear();
        }

        internal static GrieferJesusNPC SpawnJesus(GameLocation loc, Point centerTile)
        {
            int attempts = 0;
            while (attempts < 40)
            {
                attempts++;
                int minDist = 12;
                int maxDist = 18;
                int dx = Game1.random.Next(-maxDist, maxDist + 1);
                int dy = Game1.random.Next(-maxDist, maxDist + 1);
                if (Math.Abs(dx) + Math.Abs(dy) < minDist) continue;
                if (dx == 0 && dy == 0) continue;

                int tx = centerTile.X + dx;
                int ty = centerTile.Y + dy;
                var tile = new Vector2(tx, ty);
                if (!loc.isTileOnMap(tile)) continue;
                if (loc.IsTileOccupiedBy(tile)) continue;

                var pos = new Vector2(tx * 64f, ty * 64f);
                var jesus = new GrieferJesusNPC(pos, chaseSpeed: 3.5f, contactDamage: 35, health: 999999, invincible: true);
                loc.addCharacter(jesus);
                return jesus;
            }
            return null;
        }

        internal static HostileVillager SpawnDisciple(GameLocation loc, Point centerTile)
        {
            int attempts = 0;
            while (attempts < 40)
            {
                attempts++;
                int minDist = 10;
                int maxDist = 16;
                int dx = Game1.random.Next(-maxDist, maxDist + 1);
                int dy = Game1.random.Next(-maxDist, maxDist + 1);
                if (Math.Abs(dx) + Math.Abs(dy) < minDist) continue;
                if (dx == 0 && dy == 0) continue;

                int tx = centerTile.X + dx;
                int ty = centerTile.Y + dy;
                var tile = new Vector2(tx, ty);
                if (!loc.isTileOnMap(tile)) continue;
                if (loc.IsTileOccupiedBy(tile)) continue;

                var pos = new Vector2(tx * 64f, ty * 64f);
                var disciple = new HostileVillager(pos, chaseSpeed: 2.8f, contactDamage: 18, health: 120, invincible: false)
                {
                    Name = "GrieferDisciple",
                    displayName = "Disciple",
                    SimpleNonVillagerNPC = true
                };
                loc.addCharacter(disciple);
                return disciple;
            }
            return null;
        }
    }
}