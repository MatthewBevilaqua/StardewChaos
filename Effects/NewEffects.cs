using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Monsters;

namespace StardewChaos
{
    public static class PortraitModeEffect
    {
        internal static bool Active;
        private const int PortraitWidth = 749;

        public static void Start() { Active = true; Game1.playSound("cameraShutter"); }
        public static void Stop() { Active = false; }

        public static void OnRendered(object sender, RenderedEventArgs e)
        {
            if (!Active) return;
            if (Game1.currentMinigame != null) return;
            var b = e.SpriteBatch;
            var vp = Game1.graphics.GraphicsDevice.Viewport;
            int sideW = (vp.Width - PortraitWidth) / 2;
            if (sideW <= 0) return;
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, sideW, vp.Height), Color.Black);
            b.Draw(Game1.fadeToBlackRect, new Rectangle(vp.Width - sideW, 0, sideW, vp.Height), Color.Black);
        }
    }

    public static class AntiPortraitModeEffect
    {
        internal static bool Active;
        private const int PortraitWidth = 749;

        public static void Start() { Active = true; Game1.playSound("cameraShutter"); }
        public static void Stop() { Active = false; }

        public static void OnRendered(object sender, RenderedEventArgs e)
        {
            if (!Active) return;
            if (Game1.currentMinigame != null) return;
            var b = e.SpriteBatch;
            var vp = Game1.graphics.GraphicsDevice.Viewport;
            int centerX = vp.Width / 2 - PortraitWidth / 2;
            b.Draw(Game1.fadeToBlackRect, new Rectangle(centerX, 0, PortraitWidth, vp.Height), Color.Black);
        }
    }

    public static class ClearActiveEffectsEffect
    {
        public static void Execute()
        {
            int count = EffectDispatcher.ActiveTimed.Count;
            for (int i = EffectDispatcher.ActiveTimed.Count - 1; i >= 0; i--)
            {
                try { EffectDispatcher.ActiveTimed[i].OnEnd?.Invoke(); }
                catch { }
            }
            EffectDispatcher.ClearAllTimed();

            ControlEffects.StopInvert();
            ControlEffects.StopPinball();
            ControlEffects.StopNoHud();
            ScreenEffects.StopSleepy();
            ScreenEffects.StopZoomIn();
            ScreenEffects.StopIceMode();
            ScreenEffects.StopGameSpeedup();
            ScreenEffects.StopPartyTime();
            Patches.TimePatches.FreezeTime = false;
            Patches.TimePatches.PriceMultiplier = 1f;
            Patches.DamagePatches.DamageMultiplier = 1;
            PlayerEffects.SpeedMultiplier(1f);
            PlayerEffects.ResetZoom();
            NpcHideEffects.StopLonely();
            WorldEffects.StopTreeShuffle();
            WorldEffects.StopFuckUpWorld(ModEntry.Instance);
            NpcEffects.StopNpcTownSquare();
            TuddModeEffect.Stop(ModEntry.Instance);
            BaldModeManager.Stop();
            AnimorphManager.Stop();
            GrieferJesusEffects.StopJesusIsHere();

            Game1.playSound("yoba");
            try { Game1.addHUDMessage(new HUDMessage($"Cleared {count} active effect(s)!", HUDMessage.newQuest_type)); } catch { }
        }
    }

    public static class ComboTimeEffect
    {
        public static void Execute()
        {
            var mod = ModEntry.Instance;
            if (mod == null) return;
            var config = mod.Config;
            bool harmonyUsable = mod.IsHarmonyUsable();
            var activeDaily = mod.DailyManager?.ActiveDailyEffectId;
            var queuedDaily = mod.DailyManager?.QueuedDailyEffectId;
            var rng = new Random();

            for (int i = 0; i < 3; i++)
            {
                var def = mod.Registry.PickRandom(config, harmonyUsable, rng, activeDaily, queuedDaily);
                if (def != null)
                {
                    try { mod.Dispatcher.Execute(def); }
                    catch { }
                }
            }

            Game1.playSound("moneySound");
        }
    }

    public static class SpinningNpcsEffect
    {
        internal static bool Active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private static int _spinStep;
        private static readonly int[] SpinDirections = { 1, 2, 3, 0 };

        public static void Start()
        {
            Active = true;
            _ticksTotal = 60 * 60;
            _ticksRemaining = _ticksTotal;
            _spinStep = 0;
            Game1.playSound("dance");
        }

        public static void Stop()
        {
            Active = false;
            foreach (GameLocation loc in Game1.locations)
            {
                if (loc?.characters == null) continue;
                foreach (var npc in loc.characters)
                {
                    if (npc != null && npc.IsVillager)
                        npc.faceDirection(0);
                }
            }
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;

            _ticksRemaining--;
            if (_ticksRemaining <= 0) { Stop(); return; }

            if (e.IsMultipleOf(30))
            {
                _spinStep = (_spinStep + 1) % SpinDirections.Length;
                int dir = SpinDirections[_spinStep];
                var loc = Game1.currentLocation;
                if (loc != null)
                {
                    foreach (var npc in loc.characters)
                    {
                        if (npc != null && npc.IsVillager)
                            npc.faceDirection(dir);
                    }
                }
            }
        }

        public static float Progress => Active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
    }

    public static class StrongWindEffect
    {
        internal static bool Active;
        private static int _ticksRemaining;
        private static int _ticksTotal;

        public static void Start()
        {
            Active = true;
            _ticksTotal = 30 * 60;
            _ticksRemaining = _ticksTotal;
            Game1.playSound("thunder");
        }

        public static void Stop()
        {
            Active = false;
            Game1.player.xVelocity = 0;
            Game1.player.yVelocity = 0;
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;

            _ticksRemaining--;
            if (_ticksRemaining <= 0) { Stop(); return; }

            float pushX = (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 1.5) * 2.5);
            Game1.player.xVelocity += pushX * 0.15f;
        }

        public static float Progress => Active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
    }

    public static class CruiseControlEffect
    {
        internal static bool Active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private static int _lastDirection = -1;

        public static void Start()
        {
            Active = true;
            _lastDirection = -1;
            _ticksTotal = 30 * 60;
            _ticksRemaining = _ticksTotal;
            Game1.playSound("toolSwap");
        }

        public static void Stop()
        {
            Active = false;
            _lastDirection = -1;
            Patches.MovementPatches.LockedDirection = -1;
            Game1.player.xVelocity = 0;
            Game1.player.yVelocity = 0;
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;

            _ticksRemaining--;
            if (_ticksRemaining <= 0) { Stop(); return; }

            var p = Game1.player;
            if (p.movementDirections.Count > 0)
            {
                int dir = p.movementDirections[p.movementDirections.Count - 1];
                _lastDirection = dir;
            }
            else if (_lastDirection >= 0)
            {
                float speed = 4.5f;
                float dx = 0, dy = 0;
                switch (_lastDirection)
                {
                    case 0: dy = -speed; break;
                    case 1: dx = speed; break;
                    case 2: dy = speed; break;
                    case 3: dx = -speed; break;
                }

                var loc = Game1.currentLocation;
                if (loc != null)
                {
                    var bb = p.GetBoundingBox();
                    var nextBox = new Microsoft.Xna.Framework.Rectangle((int)(p.Position.X + dx), (int)(p.Position.Y + dy), bb.Width, bb.Height);
                    if (!loc.isCollidingPosition(nextBox, Game1.viewport, true, 0, false, p))
                    {
                        p.Position = new Vector2(p.Position.X + dx, p.Position.Y + dy);
                    }
                }
            }
        }

        public static float Progress => Active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
    }

    public static class EveryoneIsFarmerEffect
    {
        internal static bool Active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private static readonly Dictionary<string, string> _savedTextures = new(StringComparer.OrdinalIgnoreCase);

        public static void Start()
        {
            Active = true;
            _ticksTotal = 60 * 60;
            _ticksRemaining = _ticksTotal;
            _savedTextures.Clear();

            try
            {
                var farmerTex = Game1.content.Load<Texture2D>("Characters\\Farmer\\farmer_base");

                foreach (GameLocation loc in Game1.locations)
                {
                    if (loc?.characters == null) continue;
                    foreach (var npc in loc.characters)
                    {
                        if (npc == null || !npc.IsVillager) continue;
                        if (npc is Monster) continue;
                        _savedTextures[npc.Name] = npc.Sprite.loadedTexture ?? ("Characters\\" + npc.Name);
                        try { npc.Sprite.spriteTexture = farmerTex; } catch { }
                    }
                }
            }
            catch { }

            Game1.playSound("shwip");
        }

        public static void Stop()
        {
            if (!Active) return;
            foreach (GameLocation loc in Game1.locations)
            {
                if (loc?.characters == null) continue;
                foreach (var npc in loc.characters)
                {
                    if (npc == null) continue;
                    if (_savedTextures.TryGetValue(npc.Name, out string orig))
                    {
                        try
                        {
                            var origTex = Game1.content.Load<Texture2D>(orig);
                            npc.Sprite.spriteTexture = origTex;
                        }
                        catch { }
                    }
                }
            }
            _savedTextures.Clear();
            Active = false;
            Game1.playSound("shwip");
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;
            _ticksRemaining--;
            if (_ticksRemaining <= 0) { Stop(); return; }
        }

        public static float Progress => Active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
    }

    public static class NoInventoryEffect
    {
        internal static bool Active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private static List<Item> _savedItems;
        private static int _savedCurrentIndex;

        public static void Start()
        {
            Active = true;
            _ticksTotal = 30 * 60;
            _ticksRemaining = _ticksTotal;

            var p = Game1.player;
            _savedItems = new List<Item>(p.Items.Count);
            for (int i = 0; i < p.Items.Count; i++)
                _savedItems.Add(p.Items[i]);
            _savedCurrentIndex = p.CurrentToolIndex;

            for (int i = 0; i < p.Items.Count; i++)
                p.Items[i] = null;

            Game1.playSound("shwip");
        }

        public static void Stop()
        {
            if (!Active) return;
            var p = Game1.player;
            if (_savedItems != null)
            {
                for (int i = 0; i < p.Items.Count && i < _savedItems.Count; i++)
                    p.Items[i] = _savedItems[i];
            }
            p.CurrentToolIndex = _savedCurrentIndex;
            Active = false;
            Game1.playSound("toolSwap");
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;
            _ticksRemaining--;
            if (_ticksRemaining <= 0) { Stop(); return; }
        }

        public static float Progress => Active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
    }

    public static class ReforestationEffect
    {
        public static void Execute()
        {
            var farm = Game1.getLocationFromName("Farm") as Farm;
            if (farm == null) return;

            string[] treeTypes = { "1", "2", "6", "7", "9" };
            int placed = 0;
            int maxTrees = 40;

            for (int i = 0; i < maxTrees * 3; i++)
            {
                if (placed >= maxTrees) break;
                int x = Game1.random.Next(farm.Map.GetLayer("Back").LayerWidth);
                int y = Game1.random.Next(farm.Map.GetLayer("Back").LayerHeight);
                var tile = new Vector2(x, y);

                if (!farm.isTileOnMap(tile) || farm.IsTileOccupiedBy(tile) || farm.terrainFeatures.ContainsKey(tile))
                    continue;

                if (farm.isTilePassable(tile) && !farm.IsTileBlockedBy(tile))
                {
                    int stage = Game1.random.Next(3, 5);
                    string treeType = treeTypes[Game1.random.Next(treeTypes.Length)];
                    var tree = new Tree(treeType, stage);
                    farm.terrainFeatures.Add(tile, tree);
                    placed++;
                }
            }

            Game1.playSound("axchop");
            try { Game1.addHUDMessage(new HUDMessage($"Planted {placed} trees!", HUDMessage.newQuest_type)); } catch { }
        }
    }

    public static class ForcefieldEffect
    {
        internal static bool Active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private const int Radius = 3;

        public static void Start()
        {
            Active = true;
            _ticksTotal = 30 * 60;
            _ticksRemaining = _ticksTotal;
            Game1.playSound("powerup");
        }

        public static void Stop()
        {
            Active = false;
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;

            _ticksRemaining--;
            if (_ticksRemaining <= 0) { Stop(); return; }

            var p = Game1.player;
            var loc = Game1.currentLocation;
            if (loc == null) return;

            var pTile = p.TilePoint;
            for (int i = loc.characters.Count - 1; i >= 0; i--)
            {
                var npc = loc.characters[i];
                if (npc == null) continue;
                if (npc is Monster mon && mon.Health <= 0) continue;

                var nTile = npc.TilePoint;
                float dist = Math.Abs(nTile.X - pTile.X) + Math.Abs(nTile.Y - pTile.Y);
                if (dist <= Radius)
                {
                    float dx = npc.Position.X - p.Position.X;
                    float dy = npc.Position.Y - p.Position.Y;
                    float len = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (len < 1f) { dx = Game1.random.Next(-1, 2) * 64f; dy = Game1.random.Next(-1, 2) * 64f; len = 1f; }
                    float pushForce = 8f;
                    npc.Position = new Vector2(
                        npc.Position.X + (dx / len) * pushForce,
                        npc.Position.Y + (dy / len) * pushForce
                    );
                }
            }
        }

        public static void OnRendered(object sender, RenderedEventArgs e)
        {
            if (!Active) return;
            var b = e.SpriteBatch;
            var p = Game1.player;
            float alpha = 0.2f + 0.1f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 3);
            int radiusPx = Radius * 64;
            int cx = (int)(p.Position.X + 32 - Game1.viewport.X) - radiusPx;
            int cy = (int)(p.Position.Y - Game1.viewport.Y) - radiusPx;
            b.Draw(Game1.fadeToBlackRect, new Rectangle(cx, cy, radiusPx * 2, radiusPx * 2), Color.Cyan * alpha);
        }

        public static float Progress => Active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
    }

    public static class FenceJailEffect
    {
        internal static bool Active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private static readonly List<Vector2> _placedTiles = new();

        public static void Start()
        {
            var p = Game1.player;
            var loc = Game1.currentLocation;
            if (loc == null) return;

            _placedTiles.Clear();
            int px = p.TilePoint.X;
            int py = p.TilePoint.Y;
            int radius = 2;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Math.Abs(dx) == radius || Math.Abs(dy) == radius)
                    {
                        int tx = px + dx;
                        int ty = py + dy;
                        var tile = new Vector2(tx, ty);

                        if (!loc.isTileOnMap(tile)) continue;
                        if (loc.IsTileOccupiedBy(tile)) continue;

                        var fence = new Fence(tile, "(O)323", false);
                        if (loc.objects.TryAdd(tile, fence))
                            _placedTiles.Add(tile);
                    }
                }
            }

            Active = true;
            _ticksTotal = 15 * 60;
            _ticksRemaining = _ticksTotal;
            Game1.playSound("stoneStep");
            try { Game1.addHUDMessage(new HUDMessage("You've been jailed!", HUDMessage.error_type)); } catch { }
        }

        public static void Stop()
        {
            if (!Active) return;
            var loc = Game1.currentLocation;
            if (loc != null)
            {
                foreach (var tile in _placedTiles)
                {
                    if (loc.objects.ContainsKey(tile))
                        loc.objects.Remove(tile);
                }
            }
            _placedTiles.Clear();
            Active = false;
            Game1.playSound("stoneStep");
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;
            _ticksRemaining--;
            if (_ticksRemaining <= 0) { Stop(); return; }
        }

        public static float Progress => Active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
    }

    public static class NewGameEffect
    {
        public static void Execute()
        {
            var p = Game1.player;
            var farm = Game1.getLocationFromName("Farm") as Farm;

            ClearActiveEffectsEffect.Execute();

            p.Money = 500;
            p.health = 100;
            p.stamina = 270;

            for (int i = 0; i < p.experiencePoints.Length; i++)
                p.experiencePoints[i] = 0;

            for (int i = 0; i < p.Items.Count; i++)
                p.Items[i] = null;
            p.CurrentToolIndex = 0;

            p.friendshipData.Clear();
            p.questLog.Clear();

            if (farm != null)
            {
                farm.lastItemShipped = null;
                farm.getShippingBin(p).Clear();
                farm.animals.Clear();
                var toRemove = farm.buildings.Where(b =>
                {
                    var bt = b.buildingType?.Value ?? "";
                    return bt != "Farmhouse";
                }).ToList();
                foreach (var b in toRemove)
                    farm.buildings.Remove(b);
                WorldEffects.ClearFarm();
            }

            Game1.timeOfDay = 600;
            Game1.dayOfMonth = 1;
            Game1.currentSeason = "spring";
            Game1.year = 1;

            Game1.warpFarmer("FarmHouse", 8, 9, false);

            PlayerEffects.PlayIntroVideo();
        }
    }

    public static class CsSsEffect
    {
        internal static bool Active;
        private static Texture2D _missingTexture;
        private static readonly string[] TargetTextures =
        {
            "Maps\\spring_outdoorsTileSheet",
            "Maps\\summer_outdoorsTileSheet",
            "Maps\\fall_outdoorsTileSheet",
            "Maps\\winter_outdoorsTileSheet"
        };

        public static void Start(ModEntry mod)
        {
            try
            {
                string path = System.IO.Path.Combine(mod.Helper.DirectoryPath, "assets", "SourceMissingTexture.png");
                if (!System.IO.File.Exists(path))
                {
                    mod.Monitor.Log("CS:S: assets/SourceMissingTexture.png not found — creating default.", LogLevel.Warn);
                    CreateDefaultTexture();
                }
                else
                {
                    using var stream = System.IO.File.OpenRead(path);
                    _missingTexture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);
                }

                Active = true;
                mod.Helper.Events.Content.AssetRequested += OnAssetRequested;
                foreach (var asset in TargetTextures)
                    mod.Helper.GameContent.InvalidateCache(asset);

                Game1.playSound("thudStep");
            }
            catch (Exception e)
            {
                mod.Monitor.Log($"CS:S: failed to activate: {e.Message}", LogLevel.Error);
            }
        }

        public static void Stop(ModEntry mod)
        {
            if (!Active) return;
            Active = false;
            mod.Helper.Events.Content.AssetRequested -= OnAssetRequested;
            try
            {
                foreach (var asset in TargetTextures)
                    mod.Helper.GameContent.InvalidateCache(asset);
            }
            catch { }
            Game1.playSound("shwip");
        }

        private static void CreateDefaultTexture()
        {
            _missingTexture = new Texture2D(Game1.graphics.GraphicsDevice, 64, 64);
            var data = new Microsoft.Xna.Framework.Color[64 * 64];
            for (int i = 0; i < data.Length; i++)
            {
                int x = i % 64, y = i / 64;
                data[i] = ((x + y) % 2 == 0) ? new Microsoft.Xna.Framework.Color(255, 0, 255) : Microsoft.Xna.Framework.Color.Black;
            }
            _missingTexture.SetData(data);
        }

        private static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Active || _missingTexture == null) return;
            foreach (var asset in TargetTextures)
            {
                if (e.NameWithoutLocale.IsEquivalentTo(asset))
                {
                    e.LoadFrom(() => _missingTexture, AssetLoadPriority.Medium);
                    return;
                }
            }
        }
    }

    public static class MitosisOnDeathEffect
    {
        internal static bool Active;
        private static int _cloneCount;
        private const int MaxClones = 40;

        public static void Start()
        {
            Active = true;
            _cloneCount = 0;
            Game1.playSound("slime");
        }

        public static void Stop()
        {
            Active = false;
            _cloneCount = 0;
        }

        internal static void OnMonsterTakeDamage(Monster monster, ref int damage)
        {
            if (!Active) return;
            if (damage <= 0) return;
            if (monster.Health - damage > 0) return;
            if (_cloneCount >= MaxClones) return;
            if (monster is HostileVillager || monster is GrieferJesusNPC) return;

            var loc = Game1.currentLocation;
            if (loc == null) return;

            for (int j = 0; j < 2; j++)
            {
                try
                {
                    var pos = new Vector2(
                        monster.Position.X + Game1.random.Next(-64, 64),
                        monster.Position.Y + Game1.random.Next(-64, 64));
                    Monster clone = monster switch
                    {
                        GreenSlime => new GreenSlime(pos),
                        Bat => new Bat(pos),
                        Bug => new Bug(pos, 2),
                        Fly => new Fly(pos, false),
                        Grub => new Grub(pos),
                        DustSpirit => new DustSpirit(pos),
                        ShadowBrute => new ShadowBrute(pos),
                        Ghost => new Ghost(pos),
                        Mummy => new Mummy(pos),
                        Serpent => new Serpent(pos),
                        _ => new GreenSlime(pos)
                    };
                    clone.Health = clone.MaxHealth;
                    loc.addCharacter(clone);
                    _cloneCount++;
                }
                catch { }
            }
            Game1.playSound("slime");
        }
    }

    public class ImmortalGoatManager
    {
        private readonly ModEntry _mod;
        private bool _active;

        public ImmortalGoatManager(ModEntry mod)
        {
            _mod = mod;
        }

        public bool IsActive => _active;

        public void Activate()
        {
            _active = true;
            _mod.Monitor.Log("Immortal Goat activated.", LogLevel.Info);
            SpawnGoat();
        }

        public void Deactivate()
        {
            _active = false;
            _mod.Monitor.Log("Immortal Goat deactivated.", LogLevel.Info);
        }

        public void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!_active) return;
            SpawnGoat();
        }

        private void SpawnGoat()
        {
            try
            {
                var farm = Game1.getLocationFromName("Farm") as Farm;
                if (farm == null) return;

                var farmAnimals = farm.animals;
                if (farmAnimals == null) return;

                var goat = new FarmAnimal("Goat", Game1.random.NextInt64(), Game1.player.UniqueMultiplayerID);
                goat.displayName = "Immortal Goat";
                farm.animals.Add(goat.myID.Value, goat);

                Game1.playSound("goat");
                try { Game1.addHUDMessage(new HUDMessage("An Immortal Goat has appeared!", HUDMessage.newQuest_type)); } catch { }
            }
            catch (Exception ex)
            {
                _mod.Monitor.Log($"Immortal Goat spawn failed: {ex.Message}", LogLevel.Error);
            }
        }
    }

    public static class NewBarnEffect
    {
        public static void Execute()
        {
            var farm = Game1.getLocationFromName("Farm") as Farm;
            if (farm == null) return;

            int barnW = 7;
            int barnH = 4;
            int edgeBuffer = 3;

            int bestX = -1, bestY = -1;
            for (int attempt = 0; attempt < 200; attempt++)
            {
                int x = Game1.random.Next(edgeBuffer, Math.Max(edgeBuffer + 1, farm.Map.GetLayer("Back").LayerWidth - edgeBuffer - barnW));
                int y = Game1.random.Next(edgeBuffer, Math.Max(edgeBuffer + 1, farm.Map.GetLayer("Back").LayerHeight - edgeBuffer - barnH));

                bool valid = true;
                for (int dx = -1; dx <= barnW && valid; dx++)
                {
                    for (int dy = -1; dy <= barnH && valid; dy++)
                    {
                        var tile = new Vector2(x + dx, y + dy);
                        if (!farm.isTileOnMap(tile)) { valid = false; continue; }
                        if (farm.IsTileOccupiedBy(tile)) { valid = false; continue; }
                        if (!farm.isTilePassable(tile)) { valid = false; continue; }
                    }
                }

                if (!valid) continue;

                foreach (var building in farm.buildings)
                {
                    int bx = (int)building.tileX.Value;
                    int by = (int)building.tileY.Value;
                    int bw = (int)building.tilesWide.Value;
                    int bh = (int)building.tilesHigh.Value;
                    if (x < bx + bw + 1 && x + barnW + 1 > bx && y < by + bh + 1 && y + barnH + 1 > by)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    bestX = x;
                    bestY = y;
                    break;
                }
            }

            if (bestX < 0)
            {
                Game1.playSound("cancel");
                try { Game1.addHUDMessage(new HUDMessage("No room for a barn!", HUDMessage.error_type)); } catch { }
                return;
            }

            try
            {
                var barn = Building.CreateInstanceFromId("Barn", new Vector2(bestX, bestY));
                farm.buildings.Add(barn);
                barn.FinishConstruction();
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Monitor?.Log($"NewBarn: failed to place building: {ex.Message}", StardewModdingAPI.LogLevel.Warn);
            }

            try
            {
                string[] animalTypes = { "Goat", "Cow", "Sheep", "Pig" };
                string animalType = animalTypes[Game1.random.Next(animalTypes.Length)];
                var animal = new FarmAnimal(animalType, Game1.random.NextInt64(), Game1.player.UniqueMultiplayerID);
                farm.animals.Add(animal.myID.Value, animal);
            }
            catch { }

            Game1.playSound("cow");
            try { Game1.addHUDMessage(new HUDMessage("A barn appeared!", HUDMessage.newQuest_type)); } catch { }
        }
    }

    public class HardcoreHangmanMinigame : IMinigame
    {
        private const int ScreenW = 1280;
        private const int ScreenH = 720;
        private const int MaxWrong = 4;

        private static readonly string[] Words =
        {
            "QUARTZ","IRIDIUM","PRISMATIC","JUNIMO","MINERAL","DINOSAUR","ARTIFACT",
            "GEOLOGY","ANCIENT","FOSSIL","CRYSTAL","VOLCANO","DRAGON","SHADOW","SKELETON",
            "GRAVEYARD","SKULL","CAULDRON","SPECTACLE","MYSTERY","TREASURE","OBELISK",
            "PUMPKIN","STARFRUIT","ANCIENTFRUIT","OCTOPUS","SWORDFISH","SCORPION",
            "MUMMY","SERPENT","PEPPER","STURGEON","LAVASNAKE","HOTPOT","CACTUS"
        };

        private static readonly Random Rng = new();
        private string _word;
        private readonly HashSet<char> _guessed = new();
        private int _wrongCount;
        private bool _gameOver;
        private bool _playerWon;
        private int _endTimer;
        private int _serveDelay = 60;
        private static HardcoreHangmanMinigame _instance;

        public HardcoreHangmanMinigame()
        {
            _word = Words[Rng.Next(Words.Length)];
            _wrongCount = 0;
            _instance = this;
        }

        public static void OnButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (_instance == null || _instance._gameOver || _instance._serveDelay > 0) return;
            char c = '\0';
            if (e.Button == StardewModdingAPI.SButton.A) c = 'A';
            else if (e.Button == StardewModdingAPI.SButton.B) c = 'B';
            else if (e.Button == StardewModdingAPI.SButton.C) c = 'C';
            else if (e.Button == StardewModdingAPI.SButton.D) c = 'D';
            else if (e.Button == StardewModdingAPI.SButton.E) c = 'E';
            else if (e.Button == StardewModdingAPI.SButton.F) c = 'F';
            else if (e.Button == StardewModdingAPI.SButton.G) c = 'G';
            else if (e.Button == StardewModdingAPI.SButton.H) c = 'H';
            else if (e.Button == StardewModdingAPI.SButton.I) c = 'I';
            else if (e.Button == StardewModdingAPI.SButton.J) c = 'J';
            else if (e.Button == StardewModdingAPI.SButton.K) c = 'K';
            else if (e.Button == StardewModdingAPI.SButton.L) c = 'L';
            else if (e.Button == StardewModdingAPI.SButton.M) c = 'M';
            else if (e.Button == StardewModdingAPI.SButton.N) c = 'N';
            else if (e.Button == StardewModdingAPI.SButton.O) c = 'O';
            else if (e.Button == StardewModdingAPI.SButton.P) c = 'P';
            else if (e.Button == StardewModdingAPI.SButton.Q) c = 'Q';
            else if (e.Button == StardewModdingAPI.SButton.R) c = 'R';
            else if (e.Button == StardewModdingAPI.SButton.S) c = 'S';
            else if (e.Button == StardewModdingAPI.SButton.T) c = 'T';
            else if (e.Button == StardewModdingAPI.SButton.U) c = 'U';
            else if (e.Button == StardewModdingAPI.SButton.V) c = 'V';
            else if (e.Button == StardewModdingAPI.SButton.W) c = 'W';
            else if (e.Button == StardewModdingAPI.SButton.X) c = 'X';
            else if (e.Button == StardewModdingAPI.SButton.Y) c = 'Y';
            else if (e.Button == StardewModdingAPI.SButton.Z) c = 'Z';
            if (c != '\0')
            {
                _instance.GuessLetter(c);
                ModEntry.Instance.Helper.Input.Suppress(e.Button);
            }
        }

        public bool tick(GameTime time)
        {
            if (_gameOver) { _endTimer++; return _endTimer >= 120; }
            if (_serveDelay > 0) { _serveDelay--; return false; }
            return false;
        }

        private void GuessLetter(char c)
        {
            if (_gameOver) return;
            c = char.ToUpperInvariant(c);
            if (!char.IsLetter(c)) return;
            if (_guessed.Contains(c)) return;
            _guessed.Add(c);

            if (_word.Contains(c))
            {
                Game1.playSound("toolSwap");
                bool allRevealed = true;
                foreach (char wc in _word) { if (!_guessed.Contains(wc)) { allRevealed = false; break; } }
                if (allRevealed) { _gameOver = true; _playerWon = true; Game1.playSound("yoba"); }
            }
            else
            {
                _wrongCount++;
                Game1.playSound("dwop");
                if (_wrongCount >= MaxWrong)
                {
                    _gameOver = true;
                    _playerWon = false;
                    AdvanceTime(500);
                    try { Game1.playSound(ModEntry.LossSoundCueId); } catch { Game1.playSound("cancel"); }
                }
            }
        }

        private static void AdvanceTime(int minutes)
        {
            int newTime = Game1.timeOfDay + minutes;
            while (newTime >= 2400) newTime -= 2400;
            Game1.timeOfDay = newTime;
        }

        public void draw(SpriteBatch b)
        {
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            try
            {
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, ScreenW, ScreenH), Color.DarkRed * 0.9f);
                DrawGallows(b, _wrongCount);
                DrawWord(b);
                DrawAlphabet(b);
                DrawCenteredText(b, "HARDCORE HANGMAN", ScreenW / 2, 20, Color.Red);
                DrawCenteredText(b, $"Only {MaxWrong} wrong guesses! Lose = +5 hours!", ScreenW / 2, 55, Color.Yellow);
                if (_serveDelay > 0)
                    DrawCenteredText(b, "GET READY...", ScreenW / 2, ScreenH / 2, Color.Yellow);
                else if (_gameOver)
                {
                    if (_playerWon)
                        DrawCenteredText(b, "YOU WIN!", ScreenW / 2, ScreenH / 2 + 60, Color.LimeGreen);
                    else
                    {
                        DrawCenteredText(b, $"WORD: {_word}", ScreenW / 2, ScreenH / 2 + 40, Color.Yellow);
                        DrawCenteredText(b, "YOU LOSE  -  +5 HOURS", ScreenW / 2, ScreenH / 2 + 80, Color.Red);
                    }
                }
                else
                {
                    var bigFont = Game1.dialogueFont ?? Game1.smallFont;
                    string wrongText = $"Wrong: {_wrongCount}/{MaxWrong}";
                    var wSize = bigFont.MeasureString(wrongText);
                    b.DrawString(bigFont, wrongText, new Vector2(ScreenW / 2 - wSize.X / 2f + 2, 380 + 2), Color.Black * 0.5f);
                    b.DrawString(bigFont, wrongText, new Vector2(ScreenW / 2 - wSize.X / 2f, 380), _wrongCount >= MaxWrong - 1 ? Color.Red : Color.White);
                    DrawCenteredText(b, "Type A-Z to guess  |  Esc to forfeit", ScreenW / 2, ScreenH - 25, Color.Gray);
                }
            }
            finally { b.End(); }
        }

        private void DrawGallows(SpriteBatch b, int stage)
        {
            int gx = ScreenW / 2 - 200;
            int gy = 80;
            Color wood = new Color(139, 90, 43);
            Color body = new Color(220, 200, 180);
            b.Draw(Game1.staminaRect, new Rectangle(gx, gy + 240, 160, 12), wood);
            b.Draw(Game1.staminaRect, new Rectangle(gx + 10, gy, 12, 240), wood);
            b.Draw(Game1.staminaRect, new Rectangle(gx + 10, gy, 120, 12), wood);
            b.Draw(Game1.staminaRect, new Rectangle(gx + 128, gy, 8, 30), wood);
            if (stage >= 1) b.Draw(Game1.staminaRect, new Rectangle(gx + 120, gy + 30, 24, 24), body);
            if (stage >= 2) b.Draw(Game1.staminaRect, new Rectangle(gx + 128, gy + 54, 8, 60), body);
            if (stage >= 3) b.Draw(Game1.staminaRect, new Rectangle(gx + 108, gy + 60, 20, 6), body);
            if (stage >= 4) b.Draw(Game1.staminaRect, new Rectangle(gx + 110, gy + 114, 18, 6), body);
        }

        private void DrawWord(SpriteBatch b)
        {
            var font = Game1.dialogueFont ?? Game1.smallFont;
            int y = 420;
            int spacing = 50;
            int totalW = _word.Length * spacing;
            int startX = ScreenW / 2 - totalW / 2;
            for (int i = 0; i < _word.Length; i++)
            {
                char c = _word[i];
                int x = startX + i * spacing;
                b.Draw(Game1.staminaRect, new Rectangle(x, y + 40, spacing - 8, 4), Color.White * 0.6f);
                if (_guessed.Contains(c) || _gameOver)
                {
                    string letter = c.ToString();
                    var size = font.MeasureString(letter);
                    b.DrawString(font, letter, new Vector2(x + (spacing - 8) / 2f - size.X / 2f, y), Color.White);
                }
                else
                    b.DrawString(font, "_", new Vector2(x + (spacing - 8) / 2f - 6, y), Color.White * 0.5f);
            }
        }

        private void DrawAlphabet(SpriteBatch b)
        {
            var font = Game1.smallFont;
            int y = 560;
            int letterW = 40;
            int letterH = 32;
            int cols = 13;
            int totalW = cols * letterW;
            int startX = ScreenW / 2 - totalW / 2;
            for (int i = 0; i < 26; i++)
            {
                char c = (char)('A' + i);
                int col = i % cols;
                int row = i / cols;
                int x = startX + col * letterW;
                int yy = y + row * (letterH + 4);
                bool guessed = _guessed.Contains(c);
                bool inWord = guessed && _word.Contains(c);
                Color bg = guessed ? (inWord ? new Color(40, 80, 40) : new Color(80, 30, 30)) : new Color(30, 30, 30);
                Color fg = guessed ? (inWord ? Color.LimeGreen : Color.Red) : Color.White;
                b.Draw(Game1.staminaRect, new Rectangle(x, yy, letterW - 4, letterH), bg);
                b.Draw(Game1.staminaRect, new Rectangle(x, yy, letterW - 4, 2), Color.White * 0.3f);
                b.Draw(Game1.staminaRect, new Rectangle(x, yy + letterH - 2, letterW - 4, 2), Color.White * 0.3f);
                string letter = c.ToString();
                var size = font.MeasureString(letter);
                b.DrawString(font, letter, new Vector2(x + (letterW - 4) / 2f - size.X / 2f, yy + (letterH - size.Y) / 2f), fg);
            }
        }

        private static void DrawCenteredText(SpriteBatch b, string text, int x, int y, Color color)
        {
            SpriteFont font = Game1.smallFont;
            Vector2 size = font.MeasureString(text);
            b.DrawString(font, text, new Vector2(x - size.X / 2f, y), color);
        }

        public void receiveKeyPress(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (_serveDelay > 0) return;
            if (key == Microsoft.Xna.Framework.Input.Keys.Escape && !_gameOver)
            {
                _gameOver = true; _playerWon = false;
                AdvanceTime(500);
                try { Game1.playSound(ModEntry.LossSoundCueId); } catch { Game1.playSound("cancel"); }
                return;
            }
            if (_gameOver) return;
            if (key >= Microsoft.Xna.Framework.Input.Keys.A && key <= Microsoft.Xna.Framework.Input.Keys.Z)
            {
                char c = (char)('A' + (key - Microsoft.Xna.Framework.Input.Keys.A));
                GuessLetter(c);
            }
        }

        public void receiveKeyRelease(Microsoft.Xna.Framework.Input.Keys key) { }
        public void receiveLeftClick(int x, int y, bool playSound = true) { }
        public void releaseLeftClick(int x, int y) { }
        public void receiveRightClick(int x, int y, bool playSound = true) { }
        public void releaseRightClick(int x, int y) { }
        public void leftClickHeld(int x, int y) { }
        public void rightClickHeld(int x, int y) { }
        public bool forceQuit() { return false; }
        public void changeGameState(int which) { }
        public bool doMainGameUpdates() { return false; }
        public bool overrideFreeMouseMovement() { return true; }
        public void changeScreenSize() { }
        public void unload() { }
        public void receiveEventPoke(int data) { }
        public string minigameId() { return "HardcoreHangman"; }
    }

    public static class BlessingCircleEffect
    {
        public static void Execute()
        {
            var loc = Game1.currentLocation;
            if (loc == null) return;
            var player = Game1.player;

            var items = new[]
            {
                new { Id = "(O)472", Name = "Parsnip Seeds" },
                new { Id = "(O)473", Name = "Potato Seeds" },
                new { Id = "(O)474", Name = "Cauliflower Seeds" },
                new { Id = "(O)475", Name = "Tulip Bulb" },
                new { Id = "(O)428", Name = "Torch" },
                new { Id = "(O)390", Name = "Stone" },
                new { Id = "(O)388", Name = "Wood" },
                new { Id = "(O)771", Name = "Fiber" },
                new { Id = "(O)167", Name = "Joja Cola" },
                new { Id = "(O)129", Name = "Chub" },
                new { Id = "(O)368", Name = "Basic Fertilizer" },
                new { Id = "(O)194", Name = "Field Snack" },
            };

            int radius = 4;
            int spawned = 0;
            for (int angle = 0; angle < 360 && spawned < 10; angle += 36)
            {
                float rad = angle * (float)Math.PI / 180f;
                int tx = player.TilePoint.X + (int)(Math.Cos(rad) * radius);
                int ty = player.TilePoint.Y + (int)(Math.Sin(rad) * radius);
                var tile = new Vector2(tx, ty);

                if (!loc.isTileOnMap(tile)) continue;

                var item = items[Game1.random.Next(items.Length)];
                try
                {
                    var obj = new StardewValley.Object(item.Id, 1);
                    Game1.createItemDebris(obj, new Vector2(tx * 64f + 32f, ty * 64f + 32f), Game1.random.Next(4), loc);
                    spawned++;
                }
                catch { }
            }

            Game1.playSound("yoba");
            try { Game1.addHUDMessage(new HUDMessage($"Blessed! {spawned} items!", HUDMessage.newQuest_type)); } catch { }
        }
    }

    public static class BountifulHarvestEffect
    {
        public static void Activate()
        {
            Patches.TimePatches.PriceMultiplier = 2f;
            Game1.playSound("goldbar");
            try { Game1.addHUDMessage(new HUDMessage("Bountiful Harvest! Sell prices x2!", HUDMessage.newQuest_type)); } catch { }
        }

        public static void Deactivate()
        {
            Patches.TimePatches.PriceMultiplier = 1f;
        }
    }

    public static class AgeCropsEffect
    {
        public static void Execute()
        {
            var farm = Game1.getLocationFromName("Farm") as Farm;
            if (farm == null) return;

            int aged = 0;
            foreach (var kvp in farm.terrainFeatures.Pairs)
            {
                if (kvp.Value is HoeDirt dirt && dirt.crop != null && !dirt.crop.dead.Value)
                {
                    var crop = dirt.crop;
                    if (crop.fullyGrown.Value)
                        continue;

                    crop.dayOfCurrentPhase.Value += 1;

                    while (crop.currentPhase.Value < crop.phaseDays.Count - 1
                        && crop.dayOfCurrentPhase.Value >= crop.phaseDays[crop.currentPhase.Value])
                    {
                        crop.dayOfCurrentPhase.Value -= crop.phaseDays[crop.currentPhase.Value];
                        crop.currentPhase.Value++;
                    }

                    aged++;
                }
            }

            Game1.playSound(" wateringCan");
            try { Game1.addHUDMessage(new HUDMessage($"Aged {aged} crops!", HUDMessage.newQuest_type)); } catch { }
        }
    }

    public static class CropDustingEffect
    {
        internal static bool Active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private static Vector2 _lastPlantedTile;

        private static readonly Dictionary<string, string[]> SeasonSeeds = new()
        {
            ["spring"] = new[] { "472", "473", "474", "475", "476", "477" },
            ["summer"] = new[] { "478", "479", "480", "481", "482", "483" },
            ["fall"] = new[] { "484", "485", "486", "487", "488", "489" },
            ["winter"] = new[] { "745" }
        };

        public static void Start()
        {
            Active = true;
            _ticksTotal = 30 * 60;
            _ticksRemaining = _ticksTotal;
            _lastPlantedTile = new Vector2(-1, -1);
            Game1.playSound("dirty");
        }

        public static void Stop()
        {
            Active = false;
        }

        public static void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Active) return;
            _ticksRemaining--;
            if (_ticksRemaining <= 0) return;

            if (Game1.currentLocation == null || Game1.currentLocation.Name != "Farm") return;

            var farm = Game1.currentLocation as Farm;
            if (farm == null) return;

            var tile = Game1.player.Tile;
            if (tile == _lastPlantedTile) return;

            if (farm.terrainFeatures.TryGetValue(tile, out var feature) && feature is HoeDirt dirt && dirt.crop == null)
            {
                string season = Game1.currentSeason;
                if (!SeasonSeeds.TryGetValue(season, out var seeds))
                    seeds = SeasonSeeds["spring"];

                string seedId = seeds[Game1.random.Next(seeds.Length)];
                try
                {
                    bool planted = dirt.plant(seedId, Game1.player, false);
                    if (planted)
                        _lastPlantedTile = tile;
                }
                catch { }
            }
        }
    }

    public static class PetAllAnimalsEffect
    {
        public static void Execute()
        {
            int petted = 0;
            foreach (GameLocation loc in Game1.locations)
            {
                if (loc is Farm farm && farm.animals != null)
                {
                    foreach (var animal in farm.animals.Values)
                    {
                        if (animal != null && !animal.wasPet.Value)
                        {
                            animal.wasPet.Value = true;
                            animal.friendshipTowardFarmer.Value = Math.Min(1000, animal.friendshipTowardFarmer.Value + 15);
                            petted++;
                        }
                    }
                }
            }

            Game1.playSound("dirtyHit");
            try { Game1.addHUDMessage(new HUDMessage($"Pet {petted} animals!", HUDMessage.newQuest_type)); } catch { }
        }
    }
}