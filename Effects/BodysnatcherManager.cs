using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChaos
{
    public class BodysnatcherManager
    {
        private readonly ModEntry _mod;
        private bool _active;
        internal bool AwaitingTestSnatch;
        private class SnatchEntry
        {
            public NPC Original;
            public HostileVillager Snatcher;
            public string OriginalName;
        }
        private readonly List<SnatchEntry> _snatched = new();
        private readonly HashSet<string> _protectedNames = new();
        private int _respawnTickCounter;

        public BodysnatcherManager(ModEntry mod)
        {
            _mod = mod;
        }

        public void ArmTestSnatch()
        {
            AwaitingTestSnatch = true;
            _mod.Monitor.Log("Bodysnatch test armed: next NPC you talk to will be snatched.", LogLevel.Info);
            try { Game1.addHUDMessage(new HUDMessage("Bodysnatch test armed — talk to an NPC!", HUDMessage.error_type)); } catch { }
        }

        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!AwaitingTestSnatch) return;
            if (!e.Button.IsActionButton() || !StardewModdingAPI.Context.IsWorldReady) return;

            var loc = Game1.currentLocation;
            if (loc == null) return;

            var player = Game1.player;
            var grabTile = e.Cursor.GrabTile;
            if (grabTile == Vector2.Zero) return;

            NPC targetNpc = null;
            foreach (var c in loc.characters)
            {
                if (c == null) continue;
                if (c is StardewValley.Monsters.Monster) continue;
                if (c.SimpleNonVillagerNPC) continue;
                if (c.IsInvisible) continue;
                if (c is StardewValley.Characters.Pet) continue;
                if (c is StardewValley.Characters.Horse) continue;
                if (c is StardewValley.Characters.Child) continue;

                if (c.Tile == grabTile || c.TilePoint == new Point((int)grabTile.X, (int)grabTile.Y))
                {
                    targetNpc = c;
                    break;
                }
            }

            if (targetNpc == null)
            {
                foreach (var c in loc.characters)
                {
                    if (c == null || c.IsInvisible || c is StardewValley.Monsters.Monster) continue;
                    float dist = Math.Abs(c.TilePoint.X - player.TilePoint.X) + Math.Abs(c.TilePoint.Y - player.TilePoint.Y);
                    if (dist <= 2)
                    {
                        targetNpc = c;
                        break;
                    }
                }
            }

            if (targetNpc != null)
            {
                AwaitingTestSnatch = false;
                SnatchNpc(targetNpc);
                _mod.Monitor.Log($"Bodysnatch test: snatched {targetNpc.Name}!", LogLevel.Info);
                try { Game1.addHUDMessage(new HUDMessage($"Snatched {targetNpc.Name}!", HUDMessage.error_type)); } catch { }
            }
        }

        public void Activate()
        {
            _active = true;
            _snatched.Clear();
            _protectedNames.Clear();
            _mod.Monitor.Log("Bodysnatcher Invasion activated.", LogLevel.Info);

            SnatchInitial(0.30);
            Game1.playSound("shadowpebble");
        }

        public void Deactivate()
        {
            foreach (var entry in _snatched)
            {
                RestoreNpc(entry);
            }
            _snatched.Clear();
            _active = false;
            _mod.Monitor.Log("Bodysnatcher Invasion deactivated.", LogLevel.Info);
        }

        private void SnatchInitial(double fraction)
        {
            var villagers = new List<NPC>();
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
                    if (_protectedNames.Contains(npc.Name)) continue;
                    villagers.Add(npc);
                }
            }

            int targetCount = Math.Max(1, (int)(villagers.Count * fraction));
            var shuffled = new List<NPC>(villagers);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Game1.random.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            for (int i = 0; i < Math.Min(targetCount, shuffled.Count); i++)
            {
                SnatchNpc(shuffled[i]);
            }

            _mod.Monitor.Log($"Bodysnatcher: snatched {_snatched.Count} of {villagers.Count} villagers.", LogLevel.Info);
        }

        private void SnatchNpc(NPC original)
        {
            if (original == null) return;
            try
            {
                original.IsInvisible = true;

                var pos = original.Position;
                var tile = original.Tile;
                var snatcher = new HostileVillager(pos, chaseSpeed: 2.5f, contactDamage: 25, health: 150, invincible: false)
                {
                    Name = "Bodysnatcher_" + original.Name,
                    displayName = original.Name,
                    SimpleNonVillagerNPC = true
                };

                original.currentLocation?.addCharacter(snatcher);

                _snatched.Add(new SnatchEntry
                {
                    Original = original,
                    Snatcher = snatcher,
                    OriginalName = original.Name
                });
            }
            catch (Exception ex)
            {
                _mod.Monitor.Log($"Bodysnatcher: failed to snatch {original.Name}: {ex.Message}", LogLevel.Warn);
                try { original.IsInvisible = false; } catch { }
            }
        }

        private void RestoreNpc(SnatchEntry entry)
        {
            if (entry == null) return;
            try
            {
                if (entry.Snatcher != null)
                {
                    foreach (GameLocation loc in Game1.locations)
                    {
                        if (loc.characters.Contains(entry.Snatcher))
                        {
                            loc.characters.Remove(entry.Snatcher);
                            break;
                        }
                    }
                }
                if (entry.Original != null)
                {
                    entry.Original.IsInvisible = false;
                    _protectedNames.Add(entry.OriginalName);
                }
            }
            catch { }
        }

        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;

            _respawnTickCounter++;
            if (_respawnTickCounter < 1800) return;
            _respawnTickCounter = 0;

            try
            {
                var candidates = new List<NPC>();
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
                        if (_protectedNames.Contains(npc.Name)) continue;
                        if (npc.IsInvisible) continue;
                        bool alreadySnatched = false;
                        foreach (var entry in _snatched) { if (entry.Original == npc) { alreadySnatched = true; break; } }
                        if (alreadySnatched) continue;
                        candidates.Add(npc);
                    }
                }

                if (candidates.Count > 0)
                {
                    int count = Math.Max(1, candidates.Count / 5);
                    for (int i = 0; i < count; i++)
                    {
                        var pick = candidates[Game1.random.Next(candidates.Count)];
                        SnatchNpc(pick);
                        candidates.Remove(pick);
                        if (candidates.Count == 0) break;
                    }
                    _mod.Monitor.Log($"Bodysnatcher: respawn wave snatched {count} more.", LogLevel.Info);
                }
            }
            catch { }

            for (int i = _snatched.Count - 1; i >= 0; i--)
            {
                if (_snatched[i].Snatcher != null && _snatched[i].Snatcher.Health <= 0)
                {
                    _mod.Monitor.Log($"Bodysnatcher {_snatched[i].OriginalName} killed — restoring original.", LogLevel.Info);
                    RestoreNpc(_snatched[i]);
                    _snatched.RemoveAt(i);
                }
            }
        }
    }
}