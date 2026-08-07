using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewChaos
{
    public class EffectDef
    {
        public string Id { get; }
        public string Name { get; }
        public Difficulty Difficulty { get; }
        public bool RequiresHarmony { get; }
        public int DurationSeconds { get; }
        public bool IsDaily { get; }

        public EffectDef(string id, string name, Difficulty difficulty, bool requiresHarmony = false, int durationSeconds = 0, bool isDaily = false)
        {
            Id = id; Name = name; Difficulty = difficulty;
            RequiresHarmony = requiresHarmony; DurationSeconds = durationSeconds;
            IsDaily = isDaily;
        }
    }

    public class EffectRegistry
    {
        public List<EffectDef> All { get; } = new();

        public EffectRegistry()
        {
            // Player (no Harmony)
            All.Add(new EffectDef("TeleportRandom", "Teleport to random location", Difficulty.Basic));
            All.Add(new EffectDef("FullHeal", "Full heal", Difficulty.Basic));
            All.Add(new EffectDef("SetHp1", "Set HP to 1", Difficulty.Complex));
            All.Add(new EffectDef("SpeedDouble", "Player Speed x2", Difficulty.Basic, false, 30));
            All.Add(new EffectDef("SpeedHalf", "Player Speed x0.5", Difficulty.Basic, false, 30));
            All.Add(new EffectDef("InfiniteStamina", "Infinite stamina", Difficulty.Basic, false, 30));
            All.Add(new EffectDef("PassOutNow", "Pass out now", Difficulty.Complex));
            All.Add(new EffectDef("TeleportHome", "Teleport home", Difficulty.Basic));
            All.Add(new EffectDef("FakeTeleport", "Fake teleport", Difficulty.Complex, false, 15));

            // Economy (no Harmony)
            All.Add(new EffectDef("GoldPlus500", "Gold +500", Difficulty.Basic));
            All.Add(new EffectDef("GoldMinus500", "Gold -500", Difficulty.Complex));

            // World (no Harmony)
            All.Add(new EffectDef("ForceRain", "Force rain", Difficulty.Basic));
            All.Add(new EffectDef("SpawnSlime", "Spawn slime near player", Difficulty.Basic));
            All.Add(new EffectDef("ParsnipRain", "Parsnip rain", Difficulty.Basic));
            All.Add(new EffectDef("MessyFarm", "Messy farm", Difficulty.Complex));
            All.Add(new EffectDef("ClearFarm", "Clear farm", Difficulty.Complex));
            All.Add(new EffectDef("StopFeedingThem", "Stop feeding the cats!", Difficulty.Complex));
            All.Add(new EffectDef("GrieferJesusHere", "Griefer Jesus", Difficulty.Complex, false, 15));
            All.Add(new EffectDef("Animorph", "Animorph", Difficulty.Complex, false, 45));
            // All.Add(new EffectDef("RandomizedLocations", "Randomized locations", Difficulty.Weird, false, 180));
            All.Add(new EffectDef("BaldMode", "Bald mode", Difficulty.Test, false, 300));
            All.Add(new EffectDef("TreeShuffle", "Tree shuffle", Difficulty.Weird, false, 30));

            // NPC (no Harmony)
            All.Add(new EffectDef("NpcTeleportTownSquare", "NPCs to Town Square", Difficulty.Complex, false, 60));
            All.Add(new EffectDef("EveryoneHatesYou", "Everyone hates you", Difficulty.Weird, false, 180));
            All.Add(new EffectDef("EveryoneLovesYou", "Everyone loves you", Difficulty.Weird, false, 180));

            // Harmony-tagged
            All.Add(new EffectDef("FreezeTime", "Freeze time", Difficulty.Complex, true, 45));
            All.Add(new EffectDef("SkipTwoHours", "Skip 2 hours", Difficulty.Complex, true));
            All.Add(new EffectDef("RewindTwoHours", "Rewind 2 hours", Difficulty.Basic));
            All.Add(new EffectDef("PricesTimesTen", "Prices x10", Difficulty.Complex, true, 120));
            All.Add(new EffectDef("PricesHalved", "Prices x0.5", Difficulty.Basic, true, 120));
            All.Add(new EffectDef("ReallyStrong", "Really Strong", Difficulty.Basic, true, 60));

            // Appearance (instant, no Harmony)
            All.Add(new EffectDef("ScrambleCharacter", "Scramble character", Difficulty.Basic));
            All.Add(new EffectDef("ChangeGender", "Change gender", Difficulty.Basic));

            // Custom NPC spawn (no Harmony, despawns on day rollover)
            All.Add(new EffectDef("MysteriousStrangers", "Spawn naked people", Difficulty.Weird));

            // Inventory (instant, no Harmony)
            All.Add(new EffectDef("JunkInventory", "Junk inventory", Difficulty.Basic));
            All.Add(new EffectDef("DeleteHeldItem", "Delete held item", Difficulty.Complex));
            All.Add(new EffectDef("ShippingBinRobbed", "Shipping bin robbed", Difficulty.Complex));
            All.Add(new EffectDef("Rich", "Rich (+25k)", Difficulty.Basic));
            All.Add(new EffectDef("Poor", "Poor (0 gold)", Difficulty.Complex));
            All.Add(new EffectDef("SacrificialCircle", "Sacrificial circle", Difficulty.Complex));
            All.Add(new EffectDef("Pong", "Pong", Difficulty.Weird));
            All.Add(new EffectDef("HardcorePong", "Hardcore Pong", Difficulty.Weird));
            All.Add(new EffectDef("Frogger", "Frogger", Difficulty.Weird));
            All.Add(new EffectDef("HardFrogger", "Hardcore Frogger", Difficulty.Weird));
            All.Add(new EffectDef("Hangman", "Hangman", Difficulty.Weird));
            All.Add(new EffectDef("Tetris", "Tetris", Difficulty.Complex));
            All.Add(new EffectDef("HardcoreHangman", "Hardcore Hangman", Difficulty.Weird));
            All.Add(new EffectDef("TripleNext", "3x next effect", Difficulty.Weird));

            // Control/Screen effects
            All.Add(new EffectDef("InvertControls", "Invert controls", Difficulty.Complex, false, 30));
            All.Add(new EffectDef("Pinball", "Pinball", Difficulty.Complex, false, 20));
            All.Add(new EffectDef("NoHud", "No HUD", Difficulty.Weird, false, 45));
            All.Add(new EffectDef("Lonely", "Lonely", Difficulty.Weird, false, 120));
            All.Add(new EffectDef("TuddMode", "Engage Tudd Mode", Difficulty.Weird, false, 60));
            All.Add(new EffectDef("CruiseControl", "Cruise control", Difficulty.Complex, false, 30));
            All.Add(new EffectDef("StrongWind", "Strong wind", Difficulty.Complex, false, 30));
            All.Add(new EffectDef("Forcefield", "Forcefield", Difficulty.Complex, false, 30));
            All.Add(new EffectDef("NoInventory", "No inventory", Difficulty.Complex, false, 30));

            // New positive effects
            All.Add(new EffectDef("BlessingCircle", "Blessing circle", Difficulty.Basic));
            All.Add(new EffectDef("AgeCrops", "Age crops +1 day", Difficulty.Basic));
            All.Add(new EffectDef("CropDusting", "Crop dusting", Difficulty.Basic, false, 30));

            // Screen effects
            All.Add(new EffectDef("Sleepy", "Sleepy", Difficulty.Complex, false, 45));
            All.Add(new EffectDef("BlackBox", "Black box", Difficulty.Weird, false, 20));
            All.Add(new EffectDef("Zoom200", "Zoom 200%", Difficulty.Weird, false, 20));
            All.Add(new EffectDef("Zoom75", "Zoom 75%", Difficulty.Basic, false, 30));
            All.Add(new EffectDef("HalfTimer", "Half timer", Difficulty.Basic, false, 60));
            All.Add(new EffectDef("PartyTime", "Party time", Difficulty.Weird, false, 60));
            All.Add(new EffectDef("FuckUpWorld", "Fuck up world", Difficulty.Weird, false, 60));
            All.Add(new EffectDef("GameSpeedup", "Game speedup", Difficulty.Weird, false, 30));
            All.Add(new EffectDef("PortraitMode", "Portrait mode", Difficulty.Weird, false, 30));
            All.Add(new EffectDef("CsSs", "Forgot to Install CS:S", Difficulty.Weird, false, 30));

            // Recategorized from Test
            All.Add(new EffectDef("ClearActiveEffects", "Clear all active effects", Difficulty.Basic));
            All.Add(new EffectDef("Reforestation", "Reforestation", Difficulty.Basic));
            All.Add(new EffectDef("AntiPortraitMode", "Anti-portrait mode", Difficulty.Weird, false, 30));

            // Test effects (disabled by default, never in voting pool)
            All.Add(new EffectDef("BodysnatchTest", "Bodysnatch test", Difficulty.Test));
            All.Add(new EffectDef("ClearHotbar", "Clear hotbar", Difficulty.Test));
            All.Add(new EffectDef("ComboTime", "Combo time (3 random effects)", Difficulty.Basic));
            All.Add(new EffectDef("NewBarn", "New barn!", Difficulty.Basic));
            All.Add(new EffectDef("SpinningNpcs", "Spinning NPCs", Difficulty.Weird, false, 60));
            All.Add(new EffectDef("FenceJail", "Fence jail", Difficulty.Complex, false, 15));
            All.Add(new EffectDef("NewGame", "New game", Difficulty.Test));
            All.Add(new EffectDef("PetAllAnimals", "Pet all animals", Difficulty.Test));
            All.Add(new EffectDef("MitosisOnDeath", "Mitosis on death", Difficulty.Weird, false, 60));

            // Text effects (Harmony)
            All.Add(new EffectDef("Aaaa", "AAAA", Difficulty.Weird, true, 45));

            // Daily effects (in pool, excluded when one is active)
            All.Add(new EffectDef("IslamValley", "Islam Valley", Difficulty.Complex, false, 0, true));
            All.Add(new EffectDef("Tariffs", "Tariffs", Difficulty.Complex, true, 0, true));
            All.Add(new EffectDef("Stinky", "Ew, stinky!", Difficulty.Complex, false, 0, true));
            All.Add(new EffectDef("Bodysnatchers", "Bodysnatcher Invasion", Difficulty.Complex, false, 0, true));
            All.Add(new EffectDef("GrieferJesusOutThere", "Griefer Jesus is out there", Difficulty.Complex, false, 0, true));
            All.Add(new EffectDef("Hurricane", "Harvest Moon Hurricane", Difficulty.Complex, false, 0, true));
            All.Add(new EffectDef("ImmortalGoat", "Immortal Goat", Difficulty.Test, false, 0, true));
            All.Add(new EffectDef("BountifulHarvest", "Bountiful Harvest", Difficulty.Test, true, 0, true));
        }

        public List<EffectDef> GetEligiblePool(ChaosConfig config, bool harmonyUsable, string activeDailyEffectId = null, string queuedDailyEffectId = null)
        {
            bool dailyBlocked = activeDailyEffectId != null || queuedDailyEffectId != null;
            return All.Where(e =>
                config.IsEffectEnabled(e.Id) &&
                config.IsDifficultyEnabled(e.Difficulty) &&
                (!e.RequiresHarmony || harmonyUsable) &&
                (!e.IsDaily || !dailyBlocked)
            ).ToList();
        }

        public EffectDef PickRandom(ChaosConfig config, bool harmonyUsable, Random rng, string activeDailyEffectId = null, string queuedDailyEffectId = null)
        {
            var pool = GetEligiblePool(config, harmonyUsable, activeDailyEffectId, queuedDailyEffectId);
            if (pool.Count == 0) return null;
            return pool[rng.Next(pool.Count)];
        }
    }
}