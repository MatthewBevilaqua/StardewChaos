using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewChaos
{
    public class VotingSlot
    {
        public string Number;
        public string EffectName;
        public bool IsRandom;
        public EffectDef Effect;
    }

    public class ChaosTicker
    {
        private readonly ModEntry _mod;
        private readonly Random _rng = new();
        private int _secondsAccumulated = 0;
        internal int HalfTimerRemaining { get; set; } = 0;
        internal int HalfTimerTotal { get; } = 90;
        internal string LastEffectName { get; private set; } = "";
        internal string ActiveEffectName { get; private set; } = "";
        internal int ActiveEffectRemaining { get; private set; } = 0;
        internal int ActiveEffectTotal { get; private set; } = 0;
        internal List<VotingSlot> VotingSlots { get; private set; } = new();
        private bool _useHighNumbers = false;
        internal int CurrentBaseNumber { get; private set; } = 1;

        public ChaosTicker(ModEntry mod)
        {
            _mod = mod;
        }

        public void OnOneSecondTick(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!_mod.Config.Enabled) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (_mod.ShouldPauseTimers()) return;

            int effectiveInterval = _mod.Config.IntervalSeconds;
            if (HalfTimerRemaining > 0)
                effectiveInterval = Math.Max(1, effectiveInterval / 2);

            _secondsAccumulated++;
            if (_secondsAccumulated >= effectiveInterval)
            {
                _secondsAccumulated = 0;
                FireEffect();
            }

            if (HalfTimerRemaining > 0)
                HalfTimerRemaining--;

            if (ActiveEffectRemaining > 0)
                ActiveEffectRemaining--;
            else if (!string.IsNullOrEmpty(ActiveEffectName))
                ActiveEffectName = "";
        }

        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_mod.Config.Enabled) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (_mod.ShouldPauseTimers()) return;

            EffectDispatcher.ProcessTimedEffects(e.Ticks);
        }

        internal int SecondsUntilNext() => _mod.Config.IntervalSeconds - _secondsAccumulated;

        internal void ManualTrigger(EffectDef def)
        {
            if (def == null) return;
            _mod.Monitor.Log($"Manual trigger: {def.Name} ({def.Id})", StardewModdingAPI.LogLevel.Info);
            LastEffectName = def.Name;
            _mod.Dispatcher.Execute(def);
        }

        internal void SetActive(string name, int seconds)
        {
            ActiveEffectName = name;
            ActiveEffectRemaining = seconds;
            ActiveEffectTotal = seconds;
        }

        void FireEffect()
        {
            EffectDef def = null;

            if (_mod.Config.TwitchVotingEnabled && _mod.VoteManager != null && VotingSlots.Count > 0)
            {
                int winner = _mod.VoteManager.GetWinner(VotingSlots.Count, _rng, CurrentBaseNumber);
                if (winner >= 0 && winner < VotingSlots.Count)
                {
                    var slot = VotingSlots[winner];
                    if (slot.IsRandom)
                        def = _mod.Registry.PickRandom(_mod.Config, _mod.IsHarmonyUsable(), _rng, _mod.DailyManager?.ActiveDailyEffectId, _mod.DailyManager?.QueuedDailyEffectId);
                    else
                        def = slot.Effect;

                    var counts = _mod.VoteManager.GetVoteCounts(VotingSlots.Count, CurrentBaseNumber);
                    _mod.Monitor.Log($"Twitch vote winner: slot {winner + 1} ({slot.EffectName}) with {counts[winner]} votes.", StardewModdingAPI.LogLevel.Info);
                }
                _mod.VoteManager.Reset();
            }

            if (def == null)
            {
                if (VotingSlots.Count > 0 && !VotingSlots[0].IsRandom && VotingSlots[0].Effect != null)
                    def = VotingSlots[0].Effect;
                else if (VotingSlots.Count > 0 && VotingSlots[0].IsRandom)
                    def = _mod.Registry.PickRandom(_mod.Config, _mod.IsHarmonyUsable(), _rng, _mod.DailyManager?.ActiveDailyEffectId, _mod.DailyManager?.QueuedDailyEffectId);
                else
                    def = _mod.Registry.PickRandom(_mod.Config, _mod.IsHarmonyUsable(), _rng, _mod.DailyManager?.ActiveDailyEffectId, _mod.DailyManager?.QueuedDailyEffectId);
            }

            if (def == null)
            {
                _mod.Monitor.Log("No effects eligible in current pool — skipping tick.", StardewModdingAPI.LogLevel.Warn);
                LastEffectName = "(no eligible effects)";
                _useHighNumbers = !_useHighNumbers;
                RegenerateVotingSlots();
                return;
            }

            _mod.Monitor.Log($"Triggering effect: {def.Name} ({def.Id})", StardewModdingAPI.LogLevel.Info);
            LastEffectName = def.Name;

            _mod.Dispatcher.Execute(def);

            _useHighNumbers = !_useHighNumbers;
            RegenerateVotingSlots();
        }

        void RegenerateVotingSlots()
        {
            VotingSlots.Clear();
            int baseNum = _useHighNumbers ? 5 : 1;
            CurrentBaseNumber = baseNum;
            var pool = _mod.Registry.GetEligiblePool(_mod.Config, _mod.IsHarmonyUsable(), _mod.DailyManager?.ActiveDailyEffectId, _mod.DailyManager?.QueuedDailyEffectId);
            var used = new HashSet<string>();

            for (int i = 0; i < 3; i++)
            {
                EffectDef picked = null;
                int attempts = 0;
                while (picked == null && attempts < 50)
                {
                    attempts++;
                    if (pool.Count == 0) break;
                    var candidate = pool[_rng.Next(pool.Count)];
                    if (!used.Contains(candidate.Id))
                    {
                        picked = candidate;
                        used.Add(candidate.Id);
                    }
                }
                VotingSlots.Add(new VotingSlot
                {
                    Number = (baseNum + i).ToString(),
                    EffectName = picked?.Name ?? "(none)",
                    IsRandom = false,
                    Effect = picked
                });
            }

            VotingSlots.Add(new VotingSlot
            {
                Number = (baseNum + 3).ToString(),
                EffectName = "Random effect",
                IsRandom = true,
                Effect = null
            });
        }

        internal void EnsureVotingSlots()
        {
            if (VotingSlots.Count == 0) RegenerateVotingSlots();
        }
    }
}