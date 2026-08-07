using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChaos
{
    public static class ScreenEffects
    {
        internal static bool SleepyActive;
        internal static bool ZoomInActive;
        internal static bool IceModeActive;


        public static void StartGameSpeedup()
        {
            Patches.UpdatePatches.GameSpeedupActive = true;
        }

        public static void StopGameSpeedup()
        {
            Patches.UpdatePatches.GameSpeedupActive = false;
        }

        internal static bool PartyTimeActive;
        private static int _partyHue;

        public static void StartPartyTime()
        {
            PartyTimeActive = true;
            _partyHue = 0;
            try { StardewValley.Game1.playSound("dance"); } catch { }
        }

        public static void StopPartyTime()
        {
            PartyTimeActive = false;
        }

        private static int _sleepyElapsedMs;
        private const int SleepyCycleMs = 21000;
        private const int SleepyFadeOutMs = 8000;
        private const int SleepyHoldMs = 5000;
        private const int SleepyFadeInMs = 8000;

        private static int _zoomElapsedMs;
        private const int ZoomTotalMs = 20000;

        internal static int IceDirection = -1;

        public static void StartSleepy() { SleepyActive = true; _sleepyElapsedMs = 0; }
        public static void StopSleepy() { SleepyActive = false; }

        public static void StartZoomIn() { ZoomInActive = true; _zoomElapsedMs = 0; }
        public static void StopZoomIn() { ZoomInActive = false; }

        public static void StartIceMode() { IceModeActive = true; IceDirection = -1; Patches.MovementPatches.LockedDirection = -1; }
        public static void StopIceMode() { IceModeActive = false; IceDirection = -1; Patches.MovementPatches.LockedDirection = -1; Game1.player.xVelocity = 0; Game1.player.yVelocity = 0; }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (SleepyActive)
            {
                _sleepyElapsedMs += (int)(1000f / 60f);
                if (_sleepyElapsedMs >= SleepyCycleMs) _sleepyElapsedMs = 0;
            }
            if (ZoomInActive)
            {
                _zoomElapsedMs += (int)(1000f / 60f);
                if (_zoomElapsedMs >= ZoomTotalMs) _zoomElapsedMs = 0;
            }
            if (IceModeActive)
            {
                var p = Game1.player;
                var loc = Game1.currentLocation;
                if (loc == null) return;
                if (Patches.MovementPatches.LockedDirection < 0) return;

                float speed = 4f;
                float dx = 0, dy = 0;
                switch (Patches.MovementPatches.LockedDirection)
                {
                    case 0: dy = -speed; break;
                    case 1: dx = speed; break;
                    case 2: dy = speed; break;
                    case 3: dx = -speed; break;
                }

                var bb = p.GetBoundingBox();
                var nextBox = new Microsoft.Xna.Framework.Rectangle((int)(p.Position.X + dx), (int)(p.Position.Y + dy), bb.Width, bb.Height);

                if (loc.isCollidingPosition(nextBox, Game1.viewport, true, 0, false, p))
                {
                    Patches.MovementPatches.LockedDirection = -1;
                    ScreenEffects.IceDirection = -1;
                }
                else
                {
                    p.Position = new Vector2(p.Position.X + dx, p.Position.Y + dy);
                }
            }
        }

        public static void OnRendered(object sender, RenderedEventArgs e)
        {
            var b = e.SpriteBatch;
            var vp = Game1.graphics.GraphicsDevice.Viewport;

            if (SleepyActive)
            {
                float alpha = GetSleepyAlpha();
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, vp.Width, vp.Height), Color.Black * alpha);
            }

            if (ZoomInActive)
            {
                float progress = (float)_zoomElapsedMs / ZoomTotalMs;
                float holeScale;
                if (progress < 0.5f)
                    holeScale = 1f - (progress / 0.5f) * 0.9f;
                else if (progress < 0.75f)
                    holeScale = 0.1f;
                else
                    holeScale = 0.1f + ((progress - 0.75f) / 0.25f) * 0.9f;

                int holeW = (int)(vp.Width * holeScale);
                int holeH = (int)(vp.Height * holeScale);
                int cx = vp.Width / 2;
                int cy = vp.Height / 2;
                var hole = new Rectangle(cx - holeW / 2, cy - holeH / 2, holeW, holeH);

                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, vp.Width, hole.Top), Color.Black);
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, hole.Bottom, vp.Width, vp.Height - hole.Bottom), Color.Black);
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, hole.Top, hole.Left, hole.Height), Color.Black);
                b.Draw(Game1.fadeToBlackRect, new Rectangle(hole.Right, hole.Top, vp.Width - hole.Right, hole.Height), Color.Black);
            }

            if (IceModeActive)
            {
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, vp.Width, vp.Height), Color.Blue * 0.15f);
            }

            if (PartyTimeActive)
            {
                _partyHue = (_partyHue + 1) % 360;
                float hue = _partyHue / 360f;
                Color rainbow = ColorFromHSV(hue, 1f, 1f);
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, vp.Width, vp.Height), rainbow * 0.2f);
            }
        }

        private static Color ColorFromHSV(float h, float s, float v)
        {
            float r, g, b;
            int i = (int)(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);
            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }
            return new Color(r, g, b);
        }

        private static float GetSleepyAlpha()
        {
            int ms = _sleepyElapsedMs;
            if (ms < SleepyFadeOutMs)
                return (float)ms / SleepyFadeOutMs;
            if (ms < SleepyFadeOutMs + SleepyHoldMs)
                return 1f;
            if (ms < SleepyFadeOutMs + SleepyHoldMs + SleepyFadeInMs)
                return 1f - (float)(ms - SleepyFadeOutMs - SleepyHoldMs) / SleepyFadeInMs;
            return 0f;
        }
    }
}