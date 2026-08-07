using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChaos
{
    public static class ControlEffects
    {
        internal static bool InvertControlsActive;
        internal static bool PinballActive;
        internal static bool NoHudActive;


        private static float _pinballVx, _pinballVy;
        private const float PinballSpeed = 8f;

        public static void StartInvert() { InvertControlsActive = true; Patches.MovementPatches.InvertControlsActive = true; }
        public static void StopInvert() { InvertControlsActive = false; Patches.MovementPatches.InvertControlsActive = false; }

        public static void StartPinball()
        {
            PinballActive = true;

            _pinballVx = (Game1.random.Next(2) == 0 ? -1 : 1) * PinballSpeed;
            _pinballVy = (Game1.random.Next(2) == 0 ? -1 : 1) * PinballSpeed;
        }

        public static void StopPinball()
        {
            PinballActive = false;
            Game1.player.xVelocity = 0;
            Game1.player.yVelocity = 0;
        }

        public static void StartNoHud() { NoHudActive = true; Patches.HudPatches.NoHudActive = true; }
        public static void StopNoHud() { NoHudActive = false; Patches.HudPatches.NoHudActive = false; }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (PinballActive && StardewModdingAPI.Context.IsWorldReady)
            {
                var p = Game1.player;
                var loc = Game1.currentLocation;
                if (loc == null) return;

                float newX = p.Position.X + _pinballVx;
                float newY = p.Position.Y + _pinballVy;

                var bb = p.GetBoundingBox();
                var nextBox = new Microsoft.Xna.Framework.Rectangle((int)newX, (int)newY, bb.Width, bb.Height);

                if (loc.isCollidingPosition(nextBox, Game1.viewport, true, 0, false, p))
                {
                    if (Game1.random.Next(2) == 0)
                        _pinballVx = -_pinballVx;
                    else
                        _pinballVy = -_pinballVy;
                    Game1.playSound("thudStep");
                }
                else
                {
                    p.Position = new Vector2(newX, newY);
                }
            }
        }
    }
    public static class NpcHideEffects
    {
        private static readonly List<NPC> _hiddenNpcs = new();

        public static void StartLonely()
        {
            _hiddenNpcs.Clear();
            foreach (GameLocation loc in Game1.locations)
            {
                foreach (NPC npc in loc.characters)
                {
                    if (npc == null) continue;
                    if (npc.IsVillager && !npc.IsInvisible)
                    {
                        npc.IsInvisible = true;
                        _hiddenNpcs.Add(npc);
                    }
                }
            }
            Game1.playSound("dirtyHit");
        }

        public static void StopLonely()
        {
            foreach (var npc in _hiddenNpcs)
            {
                if (npc != null)
                    npc.IsInvisible = false;
            }
            _hiddenNpcs.Clear();
        }
    }

    public static class TuddModeEffect
    {
        internal static bool Active;
        private static readonly string[] OutdoorTilesheets =
        {
            "Maps\\spring_outdoorsTileSheet",
            "Maps\\summer_outdoorsTileSheet",
            "Maps\\fall_outdoorsTileSheet",
            "Maps\\winter_outdoorsTileSheet"
        };
        private const int TileW = 640;
        private const int TileH = 1024;
        private static Texture2D _tiledTexture;
        private static bool _tiled;

        private static void EnsureTiled(ModEntry mod)
        {
            if (_tiled) return;
            try
            {
                string path = System.IO.Path.Combine(mod.Helper.DirectoryPath, "assets", "fracktron_small.png");
                using var stream = System.IO.File.OpenRead(path);
                var src = Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);
                int sw = src.Width, sh = src.Height;
                var srcData = new Microsoft.Xna.Framework.Color[sw * sh];
                src.GetData(srcData);
                src.Dispose();

                _tiledTexture = new Texture2D(Game1.graphics.GraphicsDevice, TileW, TileH);
                var data = new Microsoft.Xna.Framework.Color[TileW * TileH];
                for (int y = 0; y < TileH; y++)
                    for (int x = 0; x < TileW; x++)
                        data[y * TileW + x] = srcData[(y % sh) * sw + (x % sw)];
                _tiledTexture.SetData(data);
                _tiled = true;
            }
            catch (Exception e)
            {
                mod.Monitor.Log($"TuddMode: failed to tile texture: {e.Message}", LogLevel.Error);
            }
        }

        public static void Start(ModEntry mod)
        {
            if (_tiledTexture == null || _tiledTexture.IsDisposed)
            {
                _tiled = false;
                _tiledTexture = null;
            }
            EnsureTiled(mod);
            if (!_tiled) return;
            try
            {
                Active = true;
                mod.Helper.Events.Content.AssetRequested += OnAssetRequested;
                foreach (var asset in OutdoorTilesheets)
                    mod.Helper.GameContent.InvalidateCache(asset);
            }
            catch (Exception e)
            {
                mod.Monitor.Log($"TuddMode: failed to activate: {e.Message}", LogLevel.Error);
            }
        }

        public static void Stop(ModEntry mod)
        {
            if (!Active) return;
            Active = false;
            mod.Helper.Events.Content.AssetRequested -= OnAssetRequested;
            try
            {
                foreach (var asset in OutdoorTilesheets)
                    mod.Helper.GameContent.InvalidateCache(asset);
            }
            catch { }
        }

        private static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Active || _tiledTexture == null) return;
            foreach (var asset in OutdoorTilesheets)
            {
                if (e.NameWithoutLocale.IsEquivalentTo(asset))
                {
                    e.LoadFrom(() => _tiledTexture, AssetLoadPriority.Medium);
                    return;
                }
            }
        }
    }
}