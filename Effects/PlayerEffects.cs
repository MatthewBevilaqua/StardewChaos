using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using Microsoft.Xna.Framework;

namespace StardewChaos
{
    public static class PlayerEffects
    {
        private static readonly string[] WarpLocations = new[]
        {
            "Town", "Forest", "Mountain", "Beach", "BusStop",
            "Farm", "FarmHouse", "Railroad", "Tunnel", "CommunityCenter"
        };

        public static void TeleportRandom()
        {
            var player = Game1.player;
            string dest = WarpLocations[Game1.random.Next(WarpLocations.Length)];
            var loc = Game1.getLocationFromName(dest);
            if (loc == null) return;

            int safeX = -1, safeY = -1;
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int x = Game1.random.Next(2, Math.Max(3, loc.Map.GetLayer("Back").LayerWidth - 2));
                int y = Game1.random.Next(2, Math.Max(3, loc.Map.GetLayer("Back").LayerHeight - 2));
                var tile = new Vector2(x, y);
                if (loc.isTileOnMap(tile)
                    && loc.isTilePassable(tile)
                    && !loc.IsTileBlockedBy(tile))
                {
                    bool hasNeighborOpen = false;
                    int[][] dirs = { new[] { 0, -1 }, new[] { 1, 0 }, new[] { 0, 1 }, new[] { -1, 0 } };
                    foreach (var d in dirs)
                    {
                        var nt = new Vector2(x + d[0], y + d[1]);
                        if (loc.isTileOnMap(nt) && loc.isTilePassable(nt) && !loc.IsTileBlockedBy(nt))
                        {
                            hasNeighborOpen = true;
                            break;
                        }
                    }
                    if (hasNeighborOpen)
                    {
                        safeX = x;
                        safeY = y;
                        break;
                    }
                }
            }

            if (safeX < 0)
            {
                safeX = 10;
                safeY = 10;
            }

            Game1.warpFarmer(dest, safeX, safeY, false);
        }

        public static void FullHeal()
        {
            var p = Game1.player;
            p.health = p.maxHealth;
            p.stamina = p.MaxStamina;
            Game1.playSound("healSound");
        }

        public static void SetHp1()
        {
            Game1.player.health = 1;
            Game1.playSound("ow");
        }

        public static void SpeedMultiplier(float mult)
        {
            if (mult == 1f)
                Game1.player.addedSpeed = 0;
            else if (mult >= 2f)
                Game1.player.addedSpeed = 5;
            else
                Game1.player.addedSpeed = -3;
        }

        public static void InfiniteStamina(bool enable)
        {
            if (enable)
                Game1.player.stamina = Game1.player.MaxStamina;
            else
                Game1.player.stamina = Math.Min(Game1.player.stamina, Game1.player.MaxStamina);
        }

        public static void PassOutNow()
        {
            Game1.player.startToPassOut();
            Game1.player.freezePause = 7000;
            Game1.playSound("ow");
        }

        public static void TeleportHome()
        {
            var house = Game1.getLocationFromName("FarmHouse");
            if (house == null) return;

            int safeX = -1, safeY = -1;
            for (int attempt = 0; attempt < 50; attempt++)
            {
                int x = Game1.random.Next(2, Math.Max(3, house.Map.GetLayer("Back").LayerWidth - 2));
                int y = Game1.random.Next(2, Math.Max(3, house.Map.GetLayer("Back").LayerHeight - 2));
                var tile = new Vector2(x, y);
                if (house.isTileOnMap(tile) && !house.IsTileBlockedBy(tile))
                {
                    safeX = x;
                    safeY = y;
                    break;
                }
            }
            if (safeX < 0) { safeX = 5; safeY = 5; }

            Game1.warpFarmer("FarmHouse", safeX, safeY, false);
            Game1.playSound("doorClose");
        }

        public static void FakeTeleport()
        {
            var player = Game1.player;
            string origLoc = player.currentLocation.NameOrUniqueName;
            int origX = player.TilePoint.X;
            int origY = player.TilePoint.Y;

            string dest = WarpLocations[Game1.random.Next(WarpLocations.Length)];
            var loc = Game1.getLocationFromName(dest);
            if (loc == null) return;

            int safeX = -1, safeY = -1;
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int x = Game1.random.Next(2, Math.Max(3, loc.Map.GetLayer("Back").LayerWidth - 2));
                int y = Game1.random.Next(2, Math.Max(3, loc.Map.GetLayer("Back").LayerHeight - 2));
                var tile = new Vector2(x, y);
                if (loc.isTileOnMap(tile) && loc.isTilePassable(tile) && !loc.IsTileBlockedBy(tile))
                {
                    bool hasNeighborOpen = false;
                    int[][] dirs = { new[] { 0, -1 }, new[] { 1, 0 }, new[] { 0, 1 }, new[] { -1, 0 } };
                    foreach (var d in dirs)
                    {
                        var nt = new Vector2(x + d[0], y + d[1]);
                        if (loc.isTileOnMap(nt) && loc.isTilePassable(nt) && !loc.IsTileBlockedBy(nt))
                        { hasNeighborOpen = true; break; }
                    }
                    if (hasNeighborOpen) { safeX = x; safeY = y; break; }
                }
            }
            if (safeX < 0) { safeX = 10; safeY = 10; }

            Game1.warpFarmer(dest, safeX, safeY, false);
            Game1.playSound("doorClose");

            System.Threading.Tasks.Task.Delay(10000).ContinueWith(t =>
            {
                Game1.warpFarmer(origLoc, origX, origY, false);
                Game1.playSound("doorClose");
            });
        }

        public static void GoldDelta(int amount)
        {
            int newMoney = (int)Game1.player.Money + amount;
            Game1.player.Money = Math.Max(0, newMoney);
            Game1.playSound(amount > 0 ? "moneySound" : "cancel");
        }

        public static void Rich()
        {
            Game1.player.Money += 25000;
            Game1.playSound("moneySound");
        }

        public static void Poor()
        {
            Game1.player.Money = 0;
            Game1.playSound("cancel");
        }

        public static void SetZoom(float zoom)
        {
            Game1.options.desiredBaseZoomLevel = zoom;
            Game1.updateViewportForScreenSizeChange(false, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
        }

        public static void ResetZoom()
        {
            Game1.options.desiredBaseZoomLevel = 1f;
            Game1.updateViewportForScreenSizeChange(false, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
        }

        public static void PlayIntroVideo()
        {
            try
            {
                IntroVideoTracker.Activate(Game1.player.currentLocation?.Name, Game1.player.Position);
                Game1.currentMinigame = new StardewValley.Minigames.GrandpaStory();
            }
            catch (Exception)
            {
                Game1.playSound("cancel");
            }
        }
    }

    public static class IntroVideoTracker
    {
        private static bool _active;
        private static string _returnLocName;
        private static Vector2 _returnPos;
        private static bool _wasInMinigame;

        public static void Activate(string locName, Vector2 pos)
        {
            _active = true;
            _returnLocName = locName;
            _returnPos = pos;
            _wasInMinigame = false;
        }

        public static void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!_active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;

            bool inMinigame = Game1.currentMinigame != null;
            if (inMinigame) _wasInMinigame = true;

            if (_wasInMinigame && !inMinigame)
            {
                try
                {
                    ModEntry.Instance?.Monitor?.Log("Intro video ended — restoring game state.", StardewModdingAPI.LogLevel.Info);
                    Game1.gameMode = 3;

                    if (!string.IsNullOrEmpty(_returnLocName))
                    {
                        var loc = Game1.getLocationFromName(_returnLocName);
                        if (loc != null)
                        {
                            Game1.warpFarmer(_returnLocName, (int)(_returnPos.X / 64f), (int)(_returnPos.Y / 64f), false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModEntry.Instance?.Monitor?.Log($"Intro video restore failed: {ex.Message}", StardewModdingAPI.LogLevel.Error);
                }
                _active = false;
                _wasInMinigame = false;
            }
        }
    }
}