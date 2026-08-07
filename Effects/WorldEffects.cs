using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace StardewChaos
{
    public static class WorldEffects
    {
        public static void ForceRain()
        {
            Game1.isRaining = true;
            Game1.playSound("thunder");
        }

        public static void SpawnSlime()
        {
            var loc = Game1.currentLocation;
            if (loc == null) return;

            var player = Game1.player;
            var pos = new Vector2(
                player.Position.X + Game1.random.Next(-100, 100),
                player.Position.Y + Game1.random.Next(-100, 100)
            );

            var slime = new GreenSlime(pos);
            loc.addCharacter(slime);
            Game1.playSound("slime");
        }

        public static void ParsnipRain()
        {
            var player = Game1.player;
            var loc = Game1.currentLocation;
            if (loc == null) return;

            for (int i = 0; i < 20; i++)
            {
                int tileX = player.TilePoint.X + Game1.random.Next(-8, 9);
                int tileY = player.TilePoint.Y + Game1.random.Next(-8, 9);
                var tile = new Vector2(tileX, tileY);
                if (loc.isTileOnMap(tile) && !loc.IsTileOccupiedBy(tile))
                {
                    Game1.createObjectDebris("(O)24", tileX, tileY);
                }
            }
            Game1.playSound("dirtyHit");
        }

        public static void MessyFarm()
        {
            var farm = Game1.getLocationFromName("Farm") as Farm;
            if (farm == null) return;

            int placed = 0;
            int attempts = 0;
            int maxAttempts = 200;
            int target = 60;

            while (placed < target && attempts < maxAttempts)
            {
                attempts++;
                int x = Game1.random.Next(farm.Map.GetLayer("Back").LayerWidth);
                int y = Game1.random.Next(farm.Map.GetLayer("Back").LayerHeight);
                var tile = new Vector2(x, y);

                if (!farm.isTileOnMap(tile)) continue;
                if (farm.IsTileOccupiedBy(tile)) continue;

                int roll = Game1.random.Next(3);
                bool didPlace = false;

                if (roll == 0)
                {
                    string[] debrisIds = { "(O)388", "(O)390", "(O)92", "(O)399" };
                    string debrisId = debrisIds[Game1.random.Next(debrisIds.Length)];
                    Game1.createObjectDebris(debrisId, x, y, farm);
                    didPlace = true;
                }
                else if (roll == 1)
                {
                    int boulderType = Game1.random.Next(3) == 0 ? 672 : 600;
                    int w = boulderType == 672 ? 2 : 2;
                    int h = boulderType == 672 ? 2 : 2;
                    var clump = new ResourceClump(boulderType, w, h, tile);
                    farm.resourceClumps.Add(clump);
                    didPlace = true;
                }
                else
                {
                    int stage = Game1.random.Next(5);
                    string treeId = (Game1.random.Next(3) == 0) ? "2" : "1";
                    var tree = new Tree(treeId, stage);
                    if (farm.terrainFeatures.TryAdd(tile, tree))
                        didPlace = true;
                }

                if (didPlace) placed++;
            }

            Game1.playSound("dirtyHit");
        }

        public static void ClearFarm()
        {
            var farm = Game1.getLocationFromName("Farm") as Farm;
            if (farm == null) return;

            int cleared = 0;

            cleared += farm.terrainFeatures.Count();
            farm.terrainFeatures.Clear();

            try { farm.largeTerrainFeatures.Clear(); } catch { }

            cleared += farm.objects.Count();
            farm.objects.Clear();

            try { farm.resourceClumps.Clear(); } catch { }

            try { farm.debris?.Clear(); } catch { }

            try { ModEntry.Instance?.Monitor?.Log($"ClearFarm: removed ~{cleared} items from farm.", StardewModdingAPI.LogLevel.Info); } catch { }
            Game1.playSound("axchop");
        }

        internal static bool TreeShuffleActive;
        private static int _treeShuffleTimer;

        public static void StartTreeShuffle()
        {
            TreeShuffleActive = true;
            _treeShuffleTimer = 0;
        }

        public static void StopTreeShuffle()
        {
            TreeShuffleActive = false;
        }

        public static void UpdateTreeShuffle()
        {
            if (!TreeShuffleActive) return;
            _treeShuffleTimer++;
            if (_treeShuffleTimer < 30) return;
            _treeShuffleTimer = 0;

            var loc = Game1.currentLocation;
            if (loc == null) return;

            var treeList = new System.Collections.Generic.List<(Vector2 tile, Tree tree)>();
            foreach (var kvp in loc.terrainFeatures.Pairs)
            {
                if (kvp.Value is Tree tree)
                    treeList.Add((kvp.Key, tree));
            }

            if (treeList.Count == 0) return;

            int treesToMove = Math.Min(treeList.Count, 8);
            for (int i = 0; i < treesToMove; i++)
            {
                var entry = treeList[Game1.random.Next(treeList.Count)];
                var oldTile = entry.tile;
                var tree = entry.tree;

                int dx = Game1.random.Next(-2, 3);
                int dy = Game1.random.Next(-2, 3);
                if (dx == 0 && dy == 0) continue;

                var newTile = new Vector2(oldTile.X + dx, oldTile.Y + dy);
                if (!loc.isTileOnMap(newTile)) continue;
                if (loc.IsTileBlockedBy(newTile)) continue;
                if (loc.terrainFeatures.ContainsKey(newTile)) continue;

                loc.terrainFeatures.Remove(oldTile);
                tree.Tile = newTile;
                loc.terrainFeatures.Add(newTile, tree);
                treeList.Remove(entry);
            }
        }

        internal static bool FuckUpWorldActive;
        private static readonly string[] FuckUpTextures =
        {
            "Maps\\spring_outdoorsTileSheet",
            "Maps\\summer_outdoorsTileSheet",
            "Maps\\fall_outdoorsTileSheet",
            "Maps\\winter_outdoorsTileSheet"
        };
        private static Dictionary<string, Texture2D> _cachedSources;
        private const int FT_TileSize = 16;

        public static void StartFuckUpWorld(ModEntry mod)
        {
            FuckUpWorldActive = true;
            _cachedSources = new Dictionary<string, Texture2D>();
            foreach (var asset in FuckUpTextures)
            {
                try { _cachedSources[asset] = Game1.content.Load<Texture2D>(asset.Replace("\\", "/")); }
                catch { }
            }

            mod.Helper.Events.Content.AssetRequested += OnFuckUpAssetRequested;
            foreach (var asset in FuckUpTextures)
                mod.Helper.GameContent.InvalidateCache(asset);
        }

        public static void StopFuckUpWorld(ModEntry mod)
        {
            FuckUpWorldActive = false;
            _cachedSources = null;
            mod.Helper.Events.Content.AssetRequested -= OnFuckUpAssetRequested;
            foreach (var asset in FuckUpTextures)
                mod.Helper.GameContent.InvalidateCache(asset);
        }

        private static void OnFuckUpAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!FuckUpWorldActive || _cachedSources == null) return;
            foreach (var asset in FuckUpTextures)
            {
                if (e.NameWithoutLocale.IsEquivalentTo(asset))
                {
                    e.Edit(assetData =>
                    {
                        var img = assetData.AsImage();
                        var targetTex = img.Data;
                        int cols = targetTex.Width / FT_TileSize;
                        int rows = targetTex.Height / FT_TileSize;
                        var sourceKeys = new List<Texture2D>(_cachedSources.Values);

                        for (int ty = 0; ty < rows; ty++)
                        {
                            for (int tx = 0; tx < cols; tx++)
                            {
                                var srcTex = sourceKeys[Game1.random.Next(sourceKeys.Count)];
                                int srcCols = srcTex.Width / FT_TileSize;
                                int srcRows = srcTex.Height / FT_TileSize;
                                int srcTileX = Game1.random.Next(srcCols);
                                int srcTileY = Game1.random.Next(srcRows);

                                var srcRect = new Microsoft.Xna.Framework.Rectangle(srcTileX * FT_TileSize, srcTileY * FT_TileSize, FT_TileSize, FT_TileSize);
                                var dstRect = new Microsoft.Xna.Framework.Rectangle(tx * FT_TileSize, ty * FT_TileSize, FT_TileSize, FT_TileSize);
                                img.PatchImage(srcTex, srcRect, dstRect);
                            }
                        }
                    });
                    return;
                }
            }
        }

        public static void SacrificialCircle()
        {
            var loc = Game1.currentLocation;
            if (loc == null) return;
            var player = Game1.player;

            string[] enemyTypes = { "Green Slime", "Bat", "Bug", "Fly", "Grub", "Duggy", "Dust Spirit", "Ghost", "Serpent" };
            string enemyType = enemyTypes[Game1.random.Next(enemyTypes.Length)];
            if (enemyType == "Green Slime") enemyType = "GreenSlime";

            int radius = 5;
            int spawned = 0;
            for (int angle = 0; angle < 360 && spawned < 10; angle += 36)
            {
                float rad = angle * (float)Math.PI / 180f;
                int tx = player.TilePoint.X + (int)(Math.Cos(rad) * radius);
                int ty = player.TilePoint.Y + (int)(Math.Sin(rad) * radius);
                var tile = new Vector2(tx, ty);

                if (loc.isTileOnMap(tile) && !loc.IsTileBlockedBy(tile))
                {
                    var pos = new Vector2(tx * 64f, ty * 64f);
                    NPC enemy = CreateEnemy(enemyType, pos);
                    if (enemy != null)
                    {
                        loc.addCharacter(enemy);
                        spawned++;
                    }
                }
            }
            Game1.playSound("shadowpebble");
        }

        private static NPC CreateEnemy(string type, Vector2 pos)
        {
            try
            {
                return type switch
                {
                    "GreenSlime" => new GreenSlime(pos),
                    "Bat" => new Bat(pos),
                    "Bug" => new Bug(pos, 2),
                    "Fly" => new Fly(pos),
                    "Grub" => new Grub(pos),
                    "Duggy" => new Duggy(pos),
                    "Dust Spirit" => new DustSpirit(pos),
                    "Ghost" => new Ghost(pos),
                    "Serpent" => new Serpent(pos),
                    _ => new GreenSlime(pos)
                };
            }
            catch { return null; }
        }
    }
}