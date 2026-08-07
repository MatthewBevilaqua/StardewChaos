using System;
using System.Collections.Generic;
using System.IO;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewChaos
{
    public sealed class ModEntry : Mod
    {
        internal static ModEntry Instance { get; private set; }
        internal static bool HarmonyAvailable { get; private set; } = false;
        internal const string LossSoundCueId = "MatthewBevilaqua.StardewChaos_Loss";

        public ChaosConfig Config { get; private set; }
        public EffectRegistry Registry { get; private set; }
        public ChaosTicker Ticker { get; private set; }
        public ChaosHud Hud { get; private set; }
        public EffectDispatcher Dispatcher { get; private set; }
        public DailyEffectManager DailyManager { get; private set; }
        public TwitchIrcClient TwitchClient { get; private set; }
        public TwitchVoteManager VoteManager { get; private set; }
        public StinkyManager Stinky { get; private set; }
        public BodysnatcherManager Bodysnatchers { get; private set; }
        public GrieferJesusOutThereManager GrieferJesusOutThere { get; private set; }
        public HurricaneManager Hurricane { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Instance = this;

            try
            {
                Config = helper.Data.ReadJsonFile<ChaosConfig>("config.json") ?? new ChaosConfig();
            }
            catch
            {
                string configPath = Path.Combine(helper.DirectoryPath, "config.json");
                if (File.Exists(configPath))
                {
                    string raw = File.ReadAllText(configPath);
                    raw = raw.Replace("\"Easy\"", "\"Basic\"")
                              .Replace("\"Hard\"", "\"Complex\"")
                              .Replace("\"Unstable\"", "\"Weird\"");
                    File.WriteAllText(configPath, raw);
                }
                Config = helper.Data.ReadJsonFile<ChaosConfig>("config.json") ?? new ChaosConfig();
            }
            Config.MergeDefaults();
            helper.Data.WriteJsonFile("config.json", Config);

            Registry = new EffectRegistry();

            ApplyHarmony();

            Dispatcher = new EffectDispatcher(this);

            Ticker = new ChaosTicker(this);
            Hud = new ChaosHud(this);
            DailyManager = new DailyEffectManager(this);
            Stinky = new StinkyManager(this);
            Bodysnatchers = new BodysnatcherManager(this);
            GrieferJesusOutThere = new GrieferJesusOutThereManager(this);
            Hurricane = new HurricaneManager(this);

            InitTwitch();

            helper.Events.GameLoop.OneSecondUpdateTicked += Ticker.OnOneSecondTick;
            helper.Events.GameLoop.OneSecondUpdateTicked += DailyManager.IslamValley.OnOneSecondTick;
            helper.Events.GameLoop.UpdateTicked += Ticker.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += ScreenEffects.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += ControlEffects.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += GrieferJesusEffects.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += Stinky.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += Bodysnatchers.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += AnimorphManager.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += GrieferJesusOutThere.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += IntroVideoTracker.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += RandomizedLocationsManager.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += BaldModeManager.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += Hurricane.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += SpinningNpcsEffect.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += StrongWindEffect.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += CruiseControlEffect.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += ForcefieldEffect.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += FenceJailEffect.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += CropDustingEffect.OnUpdateTicked;
            helper.Events.Player.Warped += Hurricane.OnWarped;
            // helper.Events.Player.Warped += RandomizedLocationsManager.OnWarped;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.RenderedHud += Hud.OnRenderedHud;
            helper.Events.Display.Rendered += DailyManager.IslamValley.OnRendered;
            helper.Events.Display.Rendered += ScreenEffects.OnRendered;
            helper.Events.Display.Rendered += AnimorphManager.OnRendered;
            helper.Events.Display.Rendered += GrieferJesusEffects.OnRendered;
            helper.Events.Display.Rendered += BaldModeManager.OnRendered;
            helper.Events.Display.Rendered += PortraitModeEffect.OnRendered;
            helper.Events.Display.Rendered += AntiPortraitModeEffect.OnRendered;
            helper.Events.Display.Rendered += ForcefieldEffect.OnRendered;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Input.ButtonPressed += Hud.OnButtonPressed;
            helper.Events.Input.ButtonPressed += DailyManager.IslamValley.OnButtonPressed;
            helper.Events.Input.ButtonPressed += Bodysnatchers.OnButtonPressed;
            helper.Events.Input.ButtonPressed += Hurricane.OnButtonPressed;
            helper.Events.Input.ButtonPressed += HangmanMinigame.OnButtonPressed;
            helper.Events.Input.ButtonPressed += HardcoreHangmanMinigame.OnButtonPressed;
            helper.Events.Input.ButtonPressed += OnButtonPressedInvert;
            helper.Events.GameLoop.TimeChanged += DailyManager.IslamValley.OnTimeChanged;
            helper.Events.GameLoop.TimeChanged += GrieferJesusOutThere.OnTimeChanged;
            helper.Events.GameLoop.DayStarted += DailyManager.OnDayStarted;
            helper.Events.GameLoop.DayStarted += DailyManager.ImmortalGoat.OnDayStarted;

            RegisterCustomAudio();

            Monitor.Log($"Loaded ({Registry.All.Count} effects, Harmony: {(HarmonyAvailable ? "loaded" : "failed")}, Twitch: {(TwitchClient?.IsConnected == true ? "connected" : "off")})", LogLevel.Info);
        }

        void RegisterCustomAudio()
        {
            try
            {
                string lossPath = Path.Combine(Helper.DirectoryPath, "assets", "Loss.wav");
                if (!File.Exists(lossPath))
                {
                    Monitor.Log("assets/Loss.wav not found — custom loss sound disabled.", LogLevel.Warn);
                    return;
                }

                Helper.Events.Content.AssetRequested += OnAudioAssetRequested;
                Helper.GameContent.InvalidateCache("Data/AudioChanges");
                Monitor.Log("Custom audio cue registered (Loss.wav).", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Custom audio registration failed: {ex.Message}", LogLevel.Warn);
            }
        }

        void OnAudioAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/AudioChanges")) return;

            string lossPath = Path.Combine(Helper.DirectoryPath, "assets", "Loss.wav");

            e.Edit(asset =>
            {
                var data = asset.AsDictionary<string, StardewValley.GameData.AudioCueData>().Data;
                data[LossSoundCueId] = new StardewValley.GameData.AudioCueData
                {
                    Id = LossSoundCueId,
                    FilePaths = new List<string> { lossPath },
                    Category = "Sound",
                    StreamedVorbis = false,
                    Looped = false,
                    UseReverb = false
                };
            }, AssetEditPriority.Late);
        }

        void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ShouldPauseTimers()) return;
            WorldEffects.UpdateTreeShuffle();
            VoteManager?.DrainQueue();
        }

        void InitTwitch()
        {
            try
            {
                string configPath = Path.Combine(Helper.DirectoryPath, "twitch_config.txt");
                if (!File.Exists(configPath))
                {
                    Monitor.Log("No twitch_config.txt found — Twitch voting disabled.", LogLevel.Info);
                    return;
                }

                string oauth = null, nick = null, channel = null;
                foreach (string line in File.ReadAllLines(configPath))
                {
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length != 2) continue;
                    string key = parts[0].Trim();
                    string val = parts[1].Trim();
                    if (string.IsNullOrEmpty(val)) continue;
                    if (key == "TwitchOAuth") oauth = val;
                    else if (key == "TwitchUsername") nick = val;
                    else if (key == "TwitchChannel") channel = val;
                }

                if (string.IsNullOrEmpty(oauth) || string.IsNullOrEmpty(nick) || string.IsNullOrEmpty(channel))
                {
                    Monitor.Log("twitch_config.txt has empty fields — Twitch voting disabled.", LogLevel.Info);
                    return;
                }

                TwitchClient = new TwitchIrcClient(oauth, nick, channel);
                VoteManager = new TwitchVoteManager(TwitchClient);
                Monitor.Log($"Twitch IRC configured for #{channel} (user: {nick}).", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Twitch init failed: {ex.Message}", LogLevel.Error);
            }
        }

        internal void ConnectTwitch()
        {
            if (TwitchClient != null && !TwitchClient.IsConnected)
                TwitchClient.Connect();
        }

        internal void DisconnectTwitch()
        {
            if (TwitchClient != null && TwitchClient.IsConnected)
                TwitchClient.Disconnect();
            VoteManager?.Reset();
        }

        void ApplyHarmony()
        {
            if (!Config.HarmonyEnabled)
            {
                HarmonyAvailable = false;
                Monitor.Log("Harmony disabled in config — time/shop/movement/text effects will be skipped.", LogLevel.Info);
                return;
            }

            try
            {
                var harmony = new HarmonyLib.Harmony("MatthewBevilaqua.StardewChaos");
                int patchCount = 0;
                try { Patches.TimePatches.Apply(harmony); patchCount++; }
                catch (Exception ex) { Monitor.Log($"TimePatches failed: {ex.Message}", LogLevel.Warn); }
                try { Patches.ShopPatches.Apply(harmony); patchCount++; }
                catch (Exception ex) { Monitor.Log($"ShopPatches failed: {ex.Message}", LogLevel.Warn); }
                try { Patches.MovementPatches.Apply(harmony); patchCount++; }
                catch (Exception ex) { Monitor.Log($"MovementPatches failed: {ex.Message}", LogLevel.Warn); }
                try { Patches.TextPatches.Apply(harmony); patchCount++; }
                catch (Exception ex) { Monitor.Log($"TextPatches failed: {ex.Message}", LogLevel.Warn); }
                try { Patches.HudPatches.Apply(harmony); patchCount++; }
                catch (Exception ex) { Monitor.Log($"HudPatches failed: {ex.Message}", LogLevel.Warn); }
                try { Patches.UpdatePatches.Apply(harmony); patchCount++; }
                catch (Exception ex) { Monitor.Log($"UpdatePatches failed: {ex.Message}", LogLevel.Warn); }
                try { Patches.AnimorphPatches.Apply(harmony); patchCount++; }
                catch (Exception ex) { Monitor.Log($"AnimorphPatches failed: {ex.Message}", LogLevel.Warn); }
                try { Patches.MousePatches.Apply(harmony, Monitor); patchCount++; }
                catch (Exception ex) { Monitor.Log($"MousePatches failed: {ex.Message}", LogLevel.Warn); }
                try { Patches.DamagePatches.Apply(harmony); patchCount++; }
                catch (Exception ex) { Monitor.Log($"DamagePatches failed: {ex.Message}", LogLevel.Warn); }
                HarmonyAvailable = true;
                Monitor.Log($"Harmony patches applied ({patchCount}/9 groups).", LogLevel.Info);
            }
            catch (Exception e)
            {
                HarmonyAvailable = false;
                Monitor.Log($"Harmony patch apply failed — some effects auto-disabled. Reason: {e.Message}", LogLevel.Warn);
            }
        }

        internal bool IsHarmonyUsable() => HarmonyAvailable && Config.HarmonyEnabled;

        internal bool ShouldPauseTimers()
        {
            if (Game1.paused) return true;
            if (!Game1.game1.IsActiveNoOverlay
                && Game1.options.pauseWhenOutOfFocus
                && Game1.multiplayerMode == 0)
                return true;
            return false;
        }

        void OnButtonPressedInvert(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.currentMinigame != null)
            {
                if (e.Button.IsActionButton() || e.Button == SButton.Enter)
                    Helper.Input.Suppress(e.Button);
            }
        }

        void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is StardewValley.Menus.ShopMenu shop)
            {
                Patches.ShopPatches.ApplyPriceMultiplier(shop);
            }
        }
    }
}