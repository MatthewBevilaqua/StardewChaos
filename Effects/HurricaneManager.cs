using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChaos
{
    public class HurricaneManager
    {
        private readonly ModEntry _mod;
        private bool _active;

        public HurricaneManager(ModEntry mod)
        {
            _mod = mod;
        }

        public bool IsActive => _active;

        public void Activate()
        {
            try
            {
                Game1.isRaining = true;
                Game1.isLightning = true;

                if (Game1.currentLocation?.Name != "FarmHouse")
                {
                    try
                    {
                        Game1.warpFarmer("FarmHouse", 8, 9, false);
                    }
                    catch
                    {
                        try { Game1.warpFarmer("FarmHouse", 1, 5, false); } catch { }
                    }
                }

                try { Game1.addHUDMessage(new HUDMessage("HURRICANE ALERT! Stay inside!", HUDMessage.error_type)); } catch { }
                try { Game1.playSound("thunder"); } catch { }

                _active = true;
                _mod.Monitor?.Log("Harvest Moon Hurricane activated.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _mod.Monitor?.Log($"Hurricane activate failed: {ex.Message}", LogLevel.Error);
            }
        }

        public void Deactivate()
        {
            _active = false;
            Game1.isLightning = false;
            _mod.Monitor?.Log("Harvest Moon Hurricane deactivated.", LogLevel.Info);
        }

        public void OnWarped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (!_active) return;
            if (e.NewLocation == null) return;

            if (e.NewLocation.Name != "FarmHouse")
            {
                try
                {
                    Game1.warpFarmer("FarmHouse", 8, 9, false);
                    try { Game1.addHUDMessage(new HUDMessage("The hurricane blocks your path!", HUDMessage.error_type)); } catch { }
                }
                catch
                {
                    try { Game1.warpFarmer("FarmHouse", 1, 5, false); } catch { }
                }
            }
        }

        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!_active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;

            var loc = Game1.currentLocation;
            if (loc == null) return;
            if (loc.Name != "FarmHouse") return;

            if (!e.Button.IsActionButton()) return;

            var grabTile = e.Cursor.GrabTile;
            if (grabTile == Vector2.Zero) return;

            try
            {
                int tx = (int)grabTile.X;
                int ty = (int)grabTile.Y;

                foreach (string layerName in new[] { "Buildings", "Back" })
                {
                    var layer = loc.Map?.GetLayer(layerName);
                    if (layer == null) continue;
                    if (tx < 0 || ty < 0 || tx >= layer.LayerWidth || ty >= layer.LayerHeight) continue;
                    var tile = layer.Tiles[tx, ty];
                    if (tile == null) continue;

                    foreach (var prop in tile.TileIndexProperties)
                    {
                        if (prop.Key == "Warp" || prop.Key == "Portal" || prop.Key == "Action")
                        {
                            string val = prop.Value?.ToString() ?? "";
                            if (val.Contains("Warp") || val.Contains("warp"))
                            {
                                _mod.Helper.Input.Suppress(e.Button);
                                try { Game1.addHUDMessage(new HUDMessage("The hurricane blocks your path!", HUDMessage.error_type)); } catch { }
                                return;
                            }
                        }
                    }
                    if (tile.Properties != null)
                    {
                        foreach (var prop in tile.Properties)
                        {
                            if (prop.Key == "Warp" || prop.Key == "Portal")
                            {
                                _mod.Helper.Input.Suppress(e.Button);
                                try { Game1.addHUDMessage(new HUDMessage("The hurricane blocks your path!", HUDMessage.error_type)); } catch { }
                                return;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;

            try
            {
                Game1.isRaining = true;
                Game1.isLightning = true;
            }
            catch { }
        }
    }
}