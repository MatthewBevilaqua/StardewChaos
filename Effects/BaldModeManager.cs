using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChaos
{
    public static class BaldModeManager
    {
        private static bool _active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private static int _savedPlayerHair;
        private static int _savedPlayerFacialHair;
        private static readonly Dictionary<string, Texture2D> _baldTextures = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> _npcOriginalTexture = new(StringComparer.OrdinalIgnoreCase);
        private static bool _texturesLoaded;

        public static bool IsActive => _active;

        public static void LoadTextures()
        {
            if (_texturesLoaded) return;
            _texturesLoaded = true;
            var monitor = ModEntry.Instance?.Monitor;
            string[] npcNames = {
                "Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Elliott",
                "Emily", "Haley", "Harvey", "Jas", "Jodi", "Kent", "Leah",
                "Lewis", "Marnie", "Maru", "Pam", "Penny", "Pierre", "Robin",
                "Sam", "Sebastian", "Shane", "Vincent", "Willy"
            };
            foreach (string name in npcNames)
            {
                string assetName = $"assets/{name}_bald";
                try
                {
                    var tex = ModEntry.Instance.Helper.ModContent.Load<Texture2D>(assetName);
                    if (tex != null)
                    {
                        _baldTextures[name] = tex;
                        monitor?.Log($"Bald texture loaded for {name} ({tex.Width}x{tex.Height})", LogLevel.Debug);
                    }
                }
                catch (Exception ex)
                {
                    monitor?.Log($"No bald texture for {name}: {ex.Message}", LogLevel.Trace);
                }
            }
            monitor?.Log($"BaldMode: loaded {_baldTextures.Count}/{npcNames.Length} NPC bald textures", LogLevel.Info);
        }

        public static void Start()
        {
            try
            {
                LoadTextures();

                var p = Game1.player;
                _savedPlayerHair = (int)p.hair.Value;
                _savedPlayerFacialHair = (int)p.facialHair.Value;

                try { p.hair.Value = -1; } catch { }
                try { p.facialHair.Value = -1; } catch { }
                try { p.changeHat(-1); } catch { }
                p.FarmerRenderer.MarkSpriteDirty();

                _npcOriginalTexture.Clear();
                foreach (GameLocation loc in Game1.locations)
                {
                    if (loc?.characters == null) continue;
                    foreach (var npc in loc.characters)
                    {
                        if (npc == null) continue;
                        if (npc is StardewValley.Monsters.Monster) continue;
                        if (npc.SimpleNonVillagerNPC) continue;
                        if (npc is StardewValley.Characters.Pet) continue;
                        if (npc is StardewValley.Characters.Horse) continue;
                        if (npc is StardewValley.Characters.Child) continue;
                        if (_baldTextures.TryGetValue(npc.Name, out Texture2D baldTex))
                        {
                            string orig = npc.Sprite.loadedTexture ?? ("Characters\\" + npc.Name);
                            _npcOriginalTexture[npc.Name] = orig;
                            npc.Sprite.spriteTexture = baldTex;
                            ModEntry.Instance?.Monitor?.Log($"BaldMode: swapped texture for {npc.Name}", LogLevel.Debug);
                        }
                    }
                }

                _ticksTotal = 5 * 60 * 60;
                _ticksRemaining = _ticksTotal;
                _active = true;

                ModEntry.Instance?.Monitor?.Log($"Bald Mode activated: player + {_npcOriginalTexture.Count} NPCs.", LogLevel.Info);
                Game1.playSound("shwip");
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Monitor?.Log($"Bald Mode start failed: {ex.Message}", LogLevel.Error);
            }
        }

        public static void Stop()
        {
            if (!_active) return;
            try
            {
                var p = Game1.player;
                try { p.hair.Value = _savedPlayerHair; } catch { }
                try { p.facialHair.Value = _savedPlayerFacialHair; } catch { }
                p.FarmerRenderer.MarkSpriteDirty();

                foreach (GameLocation loc in Game1.locations)
                {
                    if (loc?.characters == null) continue;
                    foreach (var npc in loc.characters)
                    {
                        if (npc == null) continue;
                        if (_npcOriginalTexture.TryGetValue(npc.Name, out string origAsset))
                        {
                            try
                            {
                                var origTex = Game1.content.Load<Texture2D>(origAsset);
                                npc.Sprite.spriteTexture = origTex;
                            }
                            catch { }
                        }
                    }
                }

                _npcOriginalTexture.Clear();
                _active = false;
                ModEntry.Instance?.Monitor?.Log("Bald Mode reverted.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Monitor?.Log($"Bald Mode stop failed: {ex.Message}", LogLevel.Error);
            }
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;

            _ticksRemaining--;
            if (_ticksRemaining <= 0)
            {
                Stop();
                return;
            }
        }

        public static void OnRendered(object sender, RenderedEventArgs e)
        {
        }

        public static float Progress => _active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
    }
}