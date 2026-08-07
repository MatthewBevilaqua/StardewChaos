using System.Collections.Generic;
using StardewModdingAPI;

namespace StardewChaos
{
    public enum Difficulty
    {
        Basic,
        Complex,
        Weird,
        Test
    }

    public class ChaosConfig
    {
        public bool Enabled { get; set; } = true;
        public bool TwitchVotingEnabled { get; set; } = false;
        public int IntervalSeconds { get; set; } = 90;
        public int ConfigVersion { get; set; } = 2;
        public Dictionary<Difficulty, bool> DifficultyFilter { get; set; } = new()
        {
            { Difficulty.Basic, true },
            { Difficulty.Complex, true },
            { Difficulty.Weird, true },
            { Difficulty.Test, false }
        };
        public bool HarmonyEnabled { get; set; } = true;
        public Dictionary<string, bool> EffectOverrides { get; set; } = new();

        public bool IsDifficultyEnabled(Difficulty d)
        {
            if (DifficultyFilter == null) return true;
            return DifficultyFilter.TryGetValue(d, out bool v) ? v : true;
        }

        public bool IsEffectEnabled(string effectId)
        {
            if (EffectOverrides != null && EffectOverrides.TryGetValue(effectId, out bool v))
                return v;
            return true;
        }

        internal void MergeDefaults()
        {
            if (ConfigVersion < 1)
            {
                Enabled = false;
                ConfigVersion = 1;
            }
            if (ConfigVersion < 2)
            {
                Enabled = true;
                IntervalSeconds = 90;
                ConfigVersion = 2;
            }
            if (DifficultyFilter == null)
                DifficultyFilter = new Dictionary<Difficulty, bool>
                {
                    { Difficulty.Basic, true },
                    { Difficulty.Complex, true },
                    { Difficulty.Weird, true },
                    { Difficulty.Test, false }
                };
            else
            {
                if (!DifficultyFilter.ContainsKey(Difficulty.Basic)) DifficultyFilter[Difficulty.Basic] = true;
                if (!DifficultyFilter.ContainsKey(Difficulty.Complex)) DifficultyFilter[Difficulty.Complex] = true;
                if (!DifficultyFilter.ContainsKey(Difficulty.Weird)) DifficultyFilter[Difficulty.Weird] = true;
                if (!DifficultyFilter.ContainsKey(Difficulty.Test)) DifficultyFilter[Difficulty.Test] = false;
            }
            if (EffectOverrides == null) EffectOverrides = new Dictionary<string, bool>();
        }
    }
}