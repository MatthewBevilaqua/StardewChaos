using System;
using System.Collections.Generic;
using StardewValley;

namespace StardewChaos
{
    public class EffectDispatcher
    {
        private readonly ModEntry _mod;
        private static readonly List<TimedEffect> _activeTimed = new();
        public static IReadOnlyList<TimedEffect> ActiveTimed => _activeTimed;
        internal static int NextEffectMultiplier = 1;

        public EffectDispatcher(ModEntry mod)
        {
            _mod = mod;
        }

        public void Execute(EffectDef def)
        {
            try
            {
                if (def.Id == "TripleNext")
                {
                    NextEffectMultiplier = 3;
                    _mod.Monitor.Log("Next effect will be 3x!", StardewModdingAPI.LogLevel.Info);
                    return;
                }

                int multiplier = NextEffectMultiplier;
                NextEffectMultiplier = 1;

                if (def.IsDaily && multiplier > 1)
                {
                    _mod.Monitor.Log($"Daily effect {def.Name} with {multiplier}x — lasting 3 days instead of 1.", StardewModdingAPI.LogLevel.Info);
                }

                if (def.IsDaily)
                {
                    _mod.DailyManager.SetDailyActive(def.Id, def.Name);
                }

                switch (def.Id)
                {
                    case "TeleportRandom":
                        for (int i = 0; i < multiplier; i++)
                        {
                            if (i > 0) System.Threading.Thread.Sleep(2000);
                            PlayerEffects.TeleportRandom();
                        }
                        break;
                    case "TeleportHome": PlayerEffects.TeleportHome(); break;
                    case "FakeTeleport": PlayerEffects.FakeTeleport(); break;
                    case "FullHeal": PlayerEffects.FullHeal(); break;
                    case "SetHp1": PlayerEffects.SetHp1(); break;
                    case "SpeedDouble": StartTimed(def, multiplier, () => PlayerEffects.SpeedMultiplier(2f), () => PlayerEffects.SpeedMultiplier(1f)); break;
                    case "SpeedHalf": StartTimed(def, multiplier, () => PlayerEffects.SpeedMultiplier(0.5f), () => PlayerEffects.SpeedMultiplier(1f)); break;
                    case "InfiniteStamina": StartTimed(def, multiplier, () => PlayerEffects.InfiniteStamina(true), () => PlayerEffects.InfiniteStamina(false)); break;
                    case "PassOutNow": PlayerEffects.PassOutNow(); break;
                    case "GoldPlus500": PlayerEffects.GoldDelta(500 * multiplier); break;
                    case "GoldMinus500": PlayerEffects.GoldDelta(-500 * multiplier); break;
                    case "ForceRain": WorldEffects.ForceRain(); break;
                    case "SpawnSlime":
                        for (int i = 0; i < multiplier; i++) WorldEffects.SpawnSlime();
                        break;
                    case "ParsnipRain":
                        for (int i = 0; i < multiplier; i++) WorldEffects.ParsnipRain();
                        break;
                    case "MessyFarm":
                        for (int i = 0; i < multiplier; i++) WorldEffects.MessyFarm();
                        break;
                    case "ClearFarm": WorldEffects.ClearFarm(); break;
                    case "StopFeedingThem": PetSpawnEffects.StopFeedingThem(); break;
                    case "BodysnatchTest": _mod.Bodysnatchers.ArmTestSnatch(); break;
                    case "GrieferJesusHere": StartTimed(def, multiplier, () => GrieferJesusEffects.JesusIsHere(), () => GrieferJesusEffects.StopJesusIsHere()); break;
                    case "Animorph": StartTimed(def, multiplier, () => AnimorphManager.Start(), () => AnimorphManager.Stop()); break;
                    // case "RandomizedLocations": StartTimed(def, multiplier, () => RandomizedLocationsManager.Start(), () => RandomizedLocationsManager.Stop()); break;
                    case "BaldMode": StartTimed(def, multiplier, () => BaldModeManager.Start(), () => BaldModeManager.Stop()); break;
                    case "TreeShuffle": StartTimed(def, multiplier, () => WorldEffects.StartTreeShuffle(), () => WorldEffects.StopTreeShuffle()); break;
                    case "NpcTeleportTownSquare": StartTimed(def, multiplier, () => NpcEffects.StartNpcTownSquare(), () => NpcEffects.StopNpcTownSquare()); break;
                    case "EveryoneHatesYou": StartTimed(def, multiplier, () => NpcEffects.SetFriendship(-1000), () => NpcEffects.RestoreFriendship()); break;
                    case "EveryoneLovesYou": StartTimed(def, multiplier, () => NpcEffects.SetFriendship(1000), () => NpcEffects.RestoreFriendship()); break;
                    case "FreezeTime": StartTimed(def, multiplier, () => Patches.TimePatches.FreezeTime = true, () => Patches.TimePatches.FreezeTime = false); break;
                    case "SkipTwoHours":
                        for (int i = 0; i < multiplier; i++) TimedEffects.SkipTwoHours();
                        break;
                    case "RewindTwoHours":
                        for (int i = 0; i < multiplier; i++) TimedEffects.RewindTwoHours();
                        break;
                    case "PricesTimesTen": StartTimed(def, multiplier, () => Patches.ShopPatches.BuyPriceMultiplier = 10f, () => Patches.ShopPatches.BuyPriceMultiplier = 1f); break;
                    case "PricesHalved": StartTimed(def, multiplier, () => Patches.ShopPatches.BuyPriceMultiplier = 0.5f, () => Patches.ShopPatches.BuyPriceMultiplier = 1f); break;
                    case "ReallyStrong": StartTimed(def, multiplier, () => Patches.DamagePatches.DamageMultiplier = 10, () => Patches.DamagePatches.DamageMultiplier = 1); break;

                    case "ScrambleCharacter": AppearanceEffects.ScrambleCharacter(); break;
                    case "ChangeGender": AppearanceEffects.ChangeGender(); break;
                    case "MysteriousStrangers":
                        for (int i = 0; i < multiplier; i++) NpcSpawnEffects.MysteriousStrangers();
                        break;
                    case "JunkInventory":
                        InventoryEffects.JunkInventory();
                        if (multiplier > 1)
                        {
                            System.Threading.Tasks.Task.Delay(15000).ContinueWith(t => { InventoryEffects.JunkInventory(); });
                            if (multiplier > 2)
                                System.Threading.Tasks.Task.Delay(30000).ContinueWith(t => { InventoryEffects.JunkInventory(); });
                        }
                        break;
                    case "ClearHotbar":
                        InventoryEffects.ClearHotbar();
                        if (multiplier > 1)
                        {
                            System.Threading.Tasks.Task.Delay(15000).ContinueWith(t => { InventoryEffects.ClearHotbar(); });
                            if (multiplier > 2)
                                System.Threading.Tasks.Task.Delay(30000).ContinueWith(t => { InventoryEffects.ClearHotbar(); });
                        }
                        break;
                    case "DeleteHeldItem":
                        for (int i = 0; i < multiplier; i++) InventoryEffects.DeleteHeldItem();
                        break;
                    case "ShippingBinRobbed": InventoryEffects.ShippingBinRobbed(); break;
                    case "Rich": PlayerEffects.Rich(); break;
                    case "Poor": PlayerEffects.Poor(); break;
                    case "SacrificialCircle":
                        for (int i = 0; i < multiplier; i++) WorldEffects.SacrificialCircle();
                        break;
                    case "Pong": Game1.currentMinigame = new PongMinigame(); break;
                    case "HardcorePong": Game1.currentMinigame = new HardcorePongMinigame(); break;
                    case "Frogger": Game1.currentMinigame = new FroggerMinigame(); break;
                    case "HardFrogger": Game1.currentMinigame = new FroggerMinigame(1.5f, 2, 1, 25f, 500); break;
                    case "Hangman":
                    {
                        var hm = new HangmanMinigame();
                        Game1.currentMinigame = hm;
                        break;
                    }
                    case "Tetris": Game1.currentMinigame = new TetrisMinigame(10, 500, 2.21f, 6); break;
                    case "Sleepy": StartTimed(def, multiplier, () => ScreenEffects.StartSleepy(), () => ScreenEffects.StopSleepy()); break;
                    case "BlackBox": StartTimed(def, multiplier, () => ScreenEffects.StartZoomIn(), () => ScreenEffects.StopZoomIn()); break;
                    case "Zoom200": StartTimed(def, multiplier, () => PlayerEffects.SetZoom(2f), () => PlayerEffects.ResetZoom()); break;
                    case "Zoom75": StartTimed(def, multiplier, () => PlayerEffects.SetZoom(0.75f), () => PlayerEffects.ResetZoom()); break;
                    case "HalfTimer":
                        _mod.Ticker.HalfTimerRemaining = 90 * multiplier;
                        break;
                    case "PartyTime": StartTimed(def, multiplier, () => { NpcEffects.StartPartyTime(); ScreenEffects.StartPartyTime(); }, () => { NpcEffects.StopPartyTime(); ScreenEffects.StopPartyTime(); }); break;
                    case "FuckUpWorld": StartTimed(def, multiplier, () => WorldEffects.StartFuckUpWorld(_mod), () => WorldEffects.StopFuckUpWorld(_mod)); break;
                    case "GameSpeedup": StartTimed(def, multiplier, () => ScreenEffects.StartGameSpeedup(), () => ScreenEffects.StopGameSpeedup()); break;
                    case "Aaaa": StartTimed(def, multiplier, () => Patches.TextPatches.AaaaActive = true, () => Patches.TextPatches.AaaaActive = false); break;
                    case "Tariffs": Patches.ShopPatches.BuyPriceMultiplier = 3f; break;
                    case "InvertControls": StartTimed(def, multiplier, () => ControlEffects.StartInvert(), () => ControlEffects.StopInvert()); break;
                    case "Pinball": StartTimed(def, multiplier, () => ControlEffects.StartPinball(), () => ControlEffects.StopPinball()); break;
                    case "NoHud": StartTimed(def, multiplier, () => ControlEffects.StartNoHud(), () => ControlEffects.StopNoHud()); break;
                    case "Lonely": StartTimed(def, multiplier, () => NpcHideEffects.StartLonely(), () => NpcHideEffects.StopLonely()); break;
                    case "TuddMode": StartTimed(def, multiplier, () => TuddModeEffect.Start(_mod), () => TuddModeEffect.Stop(_mod)); break;
                    case "IslamValley": _mod.DailyManager.IslamValley.Activate(); _mod.DailyManager.IslamValley.OnDayStarted(); break;
                    case "Stinky": _mod.Stinky.Activate(); break;
                    case "Bodysnatchers": _mod.Bodysnatchers.Activate(); break;
                    case "GrieferJesusOutThere": _mod.GrieferJesusOutThere.Activate(); break;
                    case "Hurricane": _mod.Hurricane.Activate(); break;
                    case "PortraitMode": StartTimed(def, multiplier, () => PortraitModeEffect.Start(), () => PortraitModeEffect.Stop()); break;
                    case "AntiPortraitMode": StartTimed(def, multiplier, () => AntiPortraitModeEffect.Start(), () => AntiPortraitModeEffect.Stop()); break;
                    case "ComboTime": ComboTimeEffect.Execute(); break;
                    case "ClearActiveEffects": ClearActiveEffectsEffect.Execute(); break;
                    case "Forcefield": StartTimed(def, multiplier, () => ForcefieldEffect.Start(), () => ForcefieldEffect.Stop()); break;
                    case "NewBarn": NewBarnEffect.Execute(); break;
                    case "SpinningNpcs": StartTimed(def, multiplier, () => SpinningNpcsEffect.Start(), () => SpinningNpcsEffect.Stop()); break;
                    case "CsSs": StartTimed(def, multiplier, () => CsSsEffect.Start(_mod), () => CsSsEffect.Stop(_mod)); break;
                    case "CruiseControl": StartTimed(def, multiplier, () => CruiseControlEffect.Start(), () => CruiseControlEffect.Stop()); break;
                    case "StrongWind": StartTimed(def, multiplier, () => StrongWindEffect.Start(), () => StrongWindEffect.Stop()); break;
                    case "FenceJail": StartTimed(def, multiplier, () => FenceJailEffect.Start(), () => FenceJailEffect.Stop()); break;
                    case "NewGame": NewGameEffect.Execute(); break;
                    case "PetAllAnimals": PetAllAnimalsEffect.Execute(); break;
                    case "NoInventory": StartTimed(def, multiplier, () => NoInventoryEffect.Start(), () => NoInventoryEffect.Stop()); break;
                    case "MitosisOnDeath": StartTimed(def, multiplier, () => MitosisOnDeathEffect.Start(), () => MitosisOnDeathEffect.Stop()); break;
                    case "Reforestation": ReforestationEffect.Execute(); break;
                    case "HardcoreHangman": Game1.currentMinigame = new HardcoreHangmanMinigame(); break;
                    case "ImmortalGoat": _mod.DailyManager.ImmortalGoat.Activate(); _mod.DailyManager.ImmortalGoat.OnDayStarted(null, null); break;
                    case "BlessingCircle": BlessingCircleEffect.Execute(); break;
                    case "BountifulHarvest": BountifulHarvestEffect.Activate(); break;
                    case "AgeCrops": AgeCropsEffect.Execute(); break;
                    case "CropDusting": StartTimed(def, multiplier, () => CropDustingEffect.Start(), () => CropDustingEffect.Stop()); break;
                    default: _mod.Monitor.Log($"Unknown effect id: {def.Id}", StardewModdingAPI.LogLevel.Warn); break;
                }
            }
            catch (Exception e)
            {
                _mod.Monitor.Log($"Effect '{def.Id}' threw: {e.GetType().Name}: {e.Message}\n{e.StackTrace}", StardewModdingAPI.LogLevel.Error);
            }
        }

        void StartTimed(EffectDef def, int multiplier, Action onStart, Action onEnd)
        {
            onStart();
            int duration = def.DurationSeconds * multiplier;
            _activeTimed.Add(new TimedEffect { Id = def.Id, Name = multiplier > 1 ? $"{def.Name} ({multiplier}x!)" : def.Name, TotalSeconds = duration, TicksRemaining = duration * 60, OnEnd = onEnd });
        }

        void StartTimed(EffectDef def, Action onStart, Action onEnd)
        {
            StartTimed(def, 1, onStart, onEnd);
        }

        public static void ProcessTimedEffects(long currentTick)
        {
            for (int i = _activeTimed.Count - 1; i >= 0; i--)
            {
                _activeTimed[i].TicksRemaining--;
                if (_activeTimed[i].TicksRemaining <= 0)
                {
                    try { _activeTimed[i].OnEnd?.Invoke(); }
                    catch { }
                    _activeTimed.RemoveAt(i);
                }
            }
        }

        public static void ClearAllTimed()
        {
            _activeTimed.Clear();
        }

        public class TimedEffect
        {
            public string Id;
            public string Name;
            public int TotalSeconds;
            public long TicksRemaining;
            public Action OnEnd;

            public float Progress => TotalSeconds > 0 ? (float)TicksRemaining / (TotalSeconds * 60) : 0f;
        }
    }
}