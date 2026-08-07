using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChaos
{
    public class DailyEffectManager
    {
        private readonly ModEntry _mod;
        private uint _lastDaysPlayed = 0;
        internal string ActiveDailyEffectId { get; private set; } = null;
        internal string ActiveDailyEffectName { get; private set; } = "None";
        internal string QueuedDailyEffectId { get; private set; } = null;
        internal string QueuedDailyEffectName { get; private set; } = "None";
        public IslamValleyManager IslamValley { get; }
        public ImmortalGoatManager ImmortalGoat { get; }

        public DailyEffectManager(ModEntry mod)
        {
            _mod = mod;
            IslamValley = new IslamValleyManager(mod);
            ImmortalGoat = new ImmortalGoatManager(mod);
        }

        public void SetDailyActive(string id, string name)
        {
            if (ActiveDailyEffectId != null && ActiveDailyEffectId != id)
            {
                QueuedDailyEffectId = id;
                QueuedDailyEffectName = name;
                _mod.Monitor.Log($"Daily effect {name} queued for next day (replacing {ActiveDailyEffectName}).", LogLevel.Info);
                try { Game1.addHUDMessage(new HUDMessage($"Tomorrow: {name}", HUDMessage.newQuest_type)); } catch { }
                return;
            }

            ActiveDailyEffectId = id;
            ActiveDailyEffectName = name;
            _mod.Monitor.Log($"Daily effect activated: {name}", LogLevel.Info);
        }

        public void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            uint currentDays = Game1.stats.DaysPlayed;
            bool isGenuineNewDay = currentDays != _lastDaysPlayed;
            _lastDaysPlayed = currentDays;

            if (!isGenuineNewDay) return;

            _mod.Monitor.Log($"New day detected (DaysPlayed: {currentDays}) — despawning chaos NPCs.", LogLevel.Info);
            NpcSpawnEffects.DespawnAll();
            PetSpawnEffects.DespawnAll();
            GrieferJesusEffects.StopJesusIsHere();

            DeactivateDaily();

            if (QueuedDailyEffectId != null)
            {
                var queuedId = QueuedDailyEffectId;
                var queuedName = QueuedDailyEffectName;
                QueuedDailyEffectId = null;
                QueuedDailyEffectName = "None";
                _mod.Monitor.Log($"Applying queued daily effect: {queuedName}", LogLevel.Info);
                SetDailyActive(queuedId, queuedName);
            }
        }

        private void DeactivateDaily()
        {
            if (ActiveDailyEffectId == "IslamValley")
                IslamValley.Deactivate();
            if (ActiveDailyEffectId == "Tariffs")
                Patches.ShopPatches.BuyPriceMultiplier = 1;
            if (ActiveDailyEffectId == "Stinky")
                _mod.Stinky.Deactivate();
            if (ActiveDailyEffectId == "Bodysnatchers")
                _mod.Bodysnatchers.Deactivate();
            if (ActiveDailyEffectId == "GrieferJesusOutThere")
                _mod.GrieferJesusOutThere.Deactivate();
            if (ActiveDailyEffectId == "Hurricane")
                _mod.Hurricane.Deactivate();
            if (ActiveDailyEffectId == "ImmortalGoat")
                ImmortalGoat.Deactivate();
            if (ActiveDailyEffectId == "BountifulHarvest")
                BountifulHarvestEffect.Deactivate();

            ActiveDailyEffectId = null;
            ActiveDailyEffectName = "None";
        }
    }
}