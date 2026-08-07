using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StardewChaos
{
    public class ChaosHud
    {
        private readonly ModEntry _mod;
        private bool _menuOpen = false;
        private int _selectedRow = 0;
        private readonly List<string> _menuRows = new();
        private int _menuPage = 0;
        private readonly string[] _pageNames = { "Settings", "Basic", "Complex", "Weird", "Daily", "Test", "All Effects" };

        public ChaosHud(ModEntry mod) { _mod = mod; }

        private List<EffectDef> GetEffectsForPage(int page)
        {
            var all = _mod.Registry.All;
            return page switch
            {
                0 => null,
                1 => all.Where(e => e.Difficulty == Difficulty.Basic && !e.IsDaily).OrderBy(e => e.Name).ToList(),
                2 => all.Where(e => e.Difficulty == Difficulty.Complex && !e.IsDaily).OrderBy(e => e.Name).ToList(),
                3 => all.Where(e => e.Difficulty == Difficulty.Weird && !e.IsDaily).OrderBy(e => e.Name).ToList(),
                4 => all.Where(e => e.IsDaily).OrderBy(e => e.Name).ToList(),
                5 => all.Where(e => e.Difficulty == Difficulty.Test && !e.IsDaily).OrderBy(e => e.Name).ToList(),
                6 => all.OrderBy(e => e.Name).ToList(),
                _ => new List<EffectDef>()
            };
        }

        public void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (!StardewModdingAPI.Context.IsWorldReady) return;

            var b = e.SpriteBatch;
            var smallFont = Game1.smallFont;
            var bigFont = Game1.dialogueFont ?? Game1.smallFont;

            Patches.TextPatches.IsDrawingChaosHud = true;

            if (_mod.Config.Enabled)
            {
                int secondsLeft = _mod.Ticker.SecondsUntilNext();
                int total = _mod.Config.IntervalSeconds;
                float progress = (float)(total - secondsLeft) / total;

                DrawChaosSection(b, progress, smallFont, bigFont);
                DrawActiveSection(b, bigFont);
                DrawVotingSection(b, bigFont);
            }

            if (_menuOpen) DrawMenu(b);
            Patches.TextPatches.IsDrawingChaosHud = false;
        }

        private void DrawChaosSection(SpriteBatch b, float progress, SpriteFont smallFont, SpriteFont bigFont)
        {
            int x = 16;
            int barW = 440;
            int barH = 32;
            int y = 80;

            int lineY = y + barH + 8;
            bool hasLast = !string.IsNullOrEmpty(_mod.Ticker.LastEffectName);

            if (hasLast) lineY += 28 + 6;

            int bgY = y - 12;
            int bgH = lineY - bgY + 4 + 5;
            int bgX = x - 8;
            int bgW = barW + 16;
            b.Draw(Game1.fadeToBlackRect, new Rectangle(bgX, bgY, bgW, bgH), Color.Black * 0.35f);

            int filled = (int)(barW * progress);
            b.Draw(Game1.fadeToBlackRect, new Rectangle(x, y, barW, barH), Color.Black * 0.6f);
            b.Draw(Game1.staminaRect, new Rectangle(x, y, filled, barH), Color.OrangeRed);
            b.Draw(Game1.staminaRect, new Rectangle(x, y, barW, 2), Color.White * 0.6f);
            b.Draw(Game1.staminaRect, new Rectangle(x, y + barH - 2, barW, 2), Color.White * 0.6f);
            b.Draw(Game1.staminaRect, new Rectangle(x, y, 2, barH), Color.White * 0.6f);
            b.Draw(Game1.staminaRect, new Rectangle(x + barW - 2, y, 2, barH), Color.White * 0.6f);

            int contentY = y + barH + 8;
            if (hasLast)
            {
                string last = $"Last: {_mod.Ticker.LastEffectName}";
                DrawFittedBoldString(b, bigFont, smallFont, last, new Vector2(x, contentY), Color.LightCyan, barW);
            }
        }

        private void DrawActiveSection(SpriteBatch b, SpriteFont bigFont)
        {
            int x = 16;
            int barW = 440;
            int barH = 32;
            int chaosBarY = 80;
            bool hasLast = !string.IsNullOrEmpty(_mod.Ticker.LastEffectName);
            int chaosContentBottom = chaosBarY + barH + 8;
            if (hasLast) chaosContentBottom += 28 + 6;
            int chaosBgBottom = chaosContentBottom + 4 + 5;
            int gap = 16;

            int bgY = chaosBgBottom + gap;
            int bgX = x - 8;
            int bgW = barW + 16;

            string todayName = _mod.DailyManager?.ActiveDailyEffectName ?? "None";
            bool hasToday = !string.IsNullOrEmpty(todayName) && todayName != "None";
            string queuedName = _mod.DailyManager?.QueuedDailyEffectName ?? "None";
            bool hasQueued = !string.IsNullOrEmpty(queuedName) && queuedName != "None";

            var activeEffects = EffectDispatcher.ActiveTimed;
            int labelH = (int)(bigFont.MeasureString("Active").Y) + 8;

            int contentStart = bgY + labelH;
            int contentBottom = contentStart;
            int textBarGap = 8;
            int activeBarW = barW / 2;

            int bigTextH = (int)bigFont.MeasureString("Active: X").Y;
            int smallTextH = (int)Game1.smallFont.MeasureString("Active: X").Y;

            if (activeEffects.Count > 0)
            {
                foreach (var eff in activeEffects)
                {
                    string full = $"Active: {eff.Name}";
                    int textH = bigTextH;
                    if (bigFont.MeasureString(full).X > barW)
                        textH = smallTextH;
                    contentBottom += textH + textBarGap + barH + 6;
                }
            }

            if (hasToday) contentBottom += 28 + 6;
            if (hasQueued) contentBottom += 28 + 6;

            int bgH = (contentBottom - bgY) + 4;
            if (bgH < 120) bgH = 120;

            b.Draw(Game1.fadeToBlackRect, new Rectangle(bgX, bgY, bgW, bgH), Color.Black * 0.35f);

            string label = "Active";
            var labelSize = bigFont.MeasureString(label);
            float labelX = bgX + (bgW - labelSize.X) / 2f;
            DrawBoldString(b, bigFont, label, new Vector2(labelX, bgY + 6), Color.White);

            var smallFont = Game1.smallFont;
            int rowY = contentStart;
            foreach (var eff in activeEffects)
            {
                string active = $"Active: {eff.Name}";
                SpriteFont textFont = bigFont;
                float textW = bigFont.MeasureString(active).X;
                if (textW > barW)
                {
                    float smallW = smallFont.MeasureString(active).X;
                    if (smallW <= barW) textFont = smallFont;
                }
                int textH = (int)textFont.MeasureString(active).Y;
                DrawFittedBoldString(b, bigFont, smallFont, active, new Vector2(x, rowY), Color.Khaki, barW);
                rowY += textH + textBarGap;

                int activeFilled = (int)(activeBarW * eff.Progress);
                b.Draw(Game1.fadeToBlackRect, new Rectangle(x, rowY, activeBarW, barH), Color.Black * 0.6f);
                b.Draw(Game1.staminaRect, new Rectangle(x, rowY, activeFilled, barH), Color.Goldenrod);
                b.Draw(Game1.staminaRect, new Rectangle(x, rowY, activeBarW, 2), Color.White * 0.6f);
                b.Draw(Game1.staminaRect, new Rectangle(x, rowY + barH - 2, activeBarW, 2), Color.White * 0.6f);
                b.Draw(Game1.staminaRect, new Rectangle(x, rowY, 2, barH), Color.White * 0.6f);
                b.Draw(Game1.staminaRect, new Rectangle(x + activeBarW - 2, rowY, 2, barH), Color.White * 0.6f);
                rowY += barH + 6;
            }

            if (hasToday)
            {
                string today = $"Today: {todayName}";
                DrawFittedBoldString(b, bigFont, smallFont, today, new Vector2(x, rowY), Color.MediumSeaGreen, barW);
                rowY += 28 + 6;
            }

            if (hasQueued)
            {
                string tmrw = $"Tomorrow: {queuedName}";
                DrawFittedBoldString(b, bigFont, smallFont, tmrw, new Vector2(x, rowY), Color.CornflowerBlue, barW);
            }
        }

        private void DrawVotingSection(SpriteBatch b, SpriteFont bigFont)
        {
            int x = 16;
            int barW = 440;
            int barH = 32;
            int gap = 16;
            int varHeight = 120;

            _mod.Ticker.EnsureVotingSlots();
            var slots = _mod.Ticker.VotingSlots;

            int chaosBarY = 80;
            bool hasLast = !string.IsNullOrEmpty(_mod.Ticker.LastEffectName);
            int chaosContentBottom = chaosBarY + barH + 8;
            if (hasLast) chaosContentBottom += 28 + 6;
            int chaosBgBottom = chaosContentBottom + 4 + 5;

            int activeBgY = chaosBgBottom + gap;
            string todayName = _mod.DailyManager?.ActiveDailyEffectName ?? "None";
            bool hasToday = !string.IsNullOrEmpty(todayName) && todayName != "None";
            string queuedName = _mod.DailyManager?.QueuedDailyEffectName ?? "None";
            bool hasQueued = !string.IsNullOrEmpty(queuedName) && queuedName != "None";
            var activeEffects = EffectDispatcher.ActiveTimed;
            int labelH = (int)(bigFont.MeasureString("Active").Y) + 8;
            int contentStart = activeBgY + labelH;
            int activeContentBottom = contentStart;
            int textBarGap = 8;
            int activeBarW = barW / 2;
            int bigTextH = (int)bigFont.MeasureString("Active: X").Y;
            int smallTextH = (int)Game1.smallFont.MeasureString("Active: X").Y;
            if (activeEffects.Count > 0)
            {
                foreach (var eff in activeEffects)
                {
                    string full = $"Active: {eff.Name}";
                    int textH = bigTextH;
                    if (bigFont.MeasureString(full).X > barW)
                        textH = smallTextH;
                    activeContentBottom += textH + textBarGap + barH + 6;
                }
            }
            if (hasToday) activeContentBottom += 28 + 6;
            if (hasQueued) activeContentBottom += 28 + 6;
            int activeBgBottom = activeContentBottom + 4;
            int activeBgH = activeBgBottom - activeBgY;
            if (activeBgH < varHeight) activeBgBottom = activeBgY + varHeight;

            int votingOffset = 2 * (gap + varHeight);
            int bgY = activeBgY + activeBgH + votingOffset;
            int bgX = x - 8;
            int voteBarZoneW = 120;
            int bgW = barW + voteBarZoneW + 24;

            int voteLabelH = (int)(bigFont.MeasureString("Next Effect").Y) + 8;
            int rowH = (int)bigFont.MeasureString("1.").Y + 6;
            int neededH = voteLabelH + (slots.Count > 0 ? slots.Count * rowH : 0) + 8;
            int bgH = Math.Max(neededH, varHeight);

            b.Draw(Game1.fadeToBlackRect, new Rectangle(bgX, bgY, bgW, bgH), Color.Black * 0.35f);

            string label = "Next Effect";
            var labelSize = bigFont.MeasureString(label);
            float labelX = bgX + (bgW - labelSize.X) / 2f;
            DrawBoldString(b, bigFont, label, new Vector2(labelX, bgY + 6), Color.White);

            int rowY = bgY + voteLabelH;
            var smallFont = Game1.smallFont;
            int[] voteCounts = null;
            if (_mod.Config.TwitchVotingEnabled && _mod.VoteManager != null && slots.Count > 0)
                voteCounts = _mod.VoteManager.GetVoteCounts(slots.Count, _mod.Ticker.CurrentBaseNumber);

            int maxVotes = 0;
            if (voteCounts != null)
            {
                foreach (var c in voteCounts) if (c > maxVotes) maxVotes = c;
            }

            int slotIdx = 0;
            int textZoneW = barW - 20;
            int barZoneX = x + textZoneW + 10;
            int barZoneW = voteBarZoneW;

            foreach (var slot in slots)
            {
                string line = $"{slot.Number}. {slot.EffectName}";
                Color color = slot.IsRandom ? Color.LightPink : Color.White;
                DrawFittedBoldString(b, bigFont, smallFont, line, new Vector2(x, rowY), color, textZoneW);

                if (voteCounts != null)
                {
                    int votes = voteCounts[slotIdx];
                    float ratio = maxVotes > 0 ? (float)votes / maxVotes : 0f;
                    int barActualW = Math.Max(1, (int)(barZoneW * ratio));
                    int vBarH = 10;
                    int barY = rowY + (rowH - vBarH) / 2;

                    b.Draw(Game1.fadeToBlackRect, new Rectangle(barZoneX, barY, barZoneW, vBarH), Color.Black * 0.5f);
                    Color barColor = slot.IsRandom ? Color.LightPink : Color.SkyBlue;
                    b.Draw(Game1.staminaRect, new Rectangle(barZoneX, barY, barActualW, vBarH), barColor);

                    string countText = votes > 0 ? votes.ToString() : "";
                    if (!string.IsNullOrEmpty(countText))
                    {
                        b.DrawString(smallFont, countText, new Vector2(barZoneX + barZoneW + 4, barY - 2), color);
                    }
                }

                rowY += rowH;
                slotIdx++;
            }
        }

        private static void DrawBoldString(SpriteBatch b, SpriteFont font, string text, Vector2 pos, Color color)
        {
            b.DrawString(font, text, pos + new Vector2(2, 2), Color.Black * 0.5f);
            b.DrawString(font, text, pos + new Vector2(1, 0), color);
            b.DrawString(font, text, pos + new Vector2(-1, 0), color);
            b.DrawString(font, text, pos, color);
        }

        private static void DrawFittedBoldString(SpriteBatch b, SpriteFont bigFont, SpriteFont smallFont, string text, Vector2 pos, Color color, int maxWidth)
        {
            float bigW = bigFont.MeasureString(text).X;
            if (bigW <= maxWidth)
            {
                DrawBoldString(b, bigFont, text, pos, color);
            }
            else
            {
                float smallW = smallFont.MeasureString(text).X;
                if (smallW <= maxWidth)
                {
                    DrawBoldString(b, smallFont, text, pos, color);
                }
                else
                {
                    float scale = maxWidth / smallW;
                    b.DrawString(smallFont, text, pos + new Vector2(2, 2), Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    b.DrawString(smallFont, text, pos + new Vector2(1, 0), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    b.DrawString(smallFont, text, pos + new Vector2(-1, 0), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    b.DrawString(smallFont, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
        }

        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.F7)
            {
                _menuOpen = !_menuOpen;
                _selectedRow = _menuPage == 0 ? 4 : 2;
                RebuildMenuRows();
            }

            if (!_menuOpen) return;

            if (e.Button == SButton.Escape) _menuOpen = false;
            else if (e.Button == SButton.Up) { do { _selectedRow--; } while (_selectedRow > 0 && string.IsNullOrEmpty(_menuRows[_selectedRow])); if (_selectedRow < 0) _selectedRow = 0; }
            else if (e.Button == SButton.Down) { do { _selectedRow++; } while (_selectedRow < _menuRows.Count - 1 && string.IsNullOrEmpty(_menuRows[_selectedRow])); if (_selectedRow >= _menuRows.Count) _selectedRow = _menuRows.Count - 1; }
            else if (e.Button == SButton.Left) { _menuPage = Math.Max(0, _menuPage - 1); _selectedRow = _menuPage == 0 ? 4 : 2; RebuildMenuRows(); }
            else if (e.Button == SButton.Right) { _menuPage = Math.Min(_pageNames.Length - 1, _menuPage + 1); _selectedRow = _menuPage == 0 ? 4 : 2; RebuildMenuRows(); }
            else if (e.Button == SButton.Enter || e.Button == SButton.Space) ToggleSelected();
            else if (e.Button == SButton.Q) TriggerSelectedEffect();
        }

        static string FormatDuration(int seconds)
        {
            if (seconds <= 0) return "";
            if (seconds < 60) return seconds + "s";
            if (seconds % 60 == 0) return (seconds / 60) + "m";
            return (seconds / 60) + "m" + (seconds % 60) + "s";
        }

        static string PadNameForPage(string name, int maxWidth)
        {
            int padding = maxWidth - name.Length;
            if (padding < 3) padding = 3;
            return name + new string(' ', padding);
        }

        void RebuildMenuRows()
        {
            _menuRows.Clear();
            var font = Game1.smallFont;

            if (_menuPage == 0)
            {
                _menuRows.Add("Stardew Valley: Chaos Mod");
                _menuRows.Add("By Matthew Bevilaqua");
                _menuRows.Add("v 1.0.0");
                _menuRows.Add("");
                _menuRows.Add($"[Mod Enabled: {_mod.Config.Enabled}]");
                _menuRows.Add($"[Interval: {_mod.Config.IntervalSeconds}s]");
                _menuRows.Add($"[Twitch Voting: {(_mod.Config.TwitchVotingEnabled ? "On" : "Off")}]");
                _menuRows.Add("[Trigger Random Effect Now]");
                _menuRows.Add($"[Basic: {_mod.Config.IsDifficultyEnabled(Difficulty.Basic)}]");
                _menuRows.Add($"[Complex: {_mod.Config.IsDifficultyEnabled(Difficulty.Complex)}]");
                _menuRows.Add($"[Weird: {_mod.Config.IsDifficultyEnabled(Difficulty.Weird)}]");
                _menuRows.Add($"[Daily: {AreDailyEffectsEnabled()}]");
                _menuRows.Add($"[Harmony: {(_mod.IsHarmonyUsable() ? "loaded" : "failed/off")}]");
                _menuRows.Add("");
            }
            else if (_menuPage == 4) // Daily page
            {
                _menuRows.Add(_pageNames[_menuPage]);
                _menuRows.Add("");
                var effects = GetEffectsForPage(_menuPage);
                if (effects != null)
                {
                    int maxW = effects.Max(e => e.Name.Length);
                    foreach (var eff in effects)
                    {
                        bool on = _mod.Config.IsEffectEnabled(eff.Id);
                        string dur = FormatDuration(eff.DurationSeconds);
                        string label = eff.IsDaily ? "(Daily)" : (dur != "" ? dur : "");
                        string padded = PadNameForPage(eff.Name, maxW);
                        _menuRows.Add($"[{(on ? "x" : " ")}] {padded}{label}");
                    }
                }
                _menuRows.Add("");
            }
            else if (_menuPage == 5) // Test page
            {
                _menuRows.Add(_pageNames[_menuPage]);
                _menuRows.Add("");
                var effects = GetEffectsForPage(_menuPage);
                if (effects != null)
                {
                    int maxW = effects.Max(e => e.Name.Length);
                    foreach (var eff in effects)
                    {
                        string dur = FormatDuration(eff.DurationSeconds);
                        string padded = PadNameForPage(eff.Name, maxW);
                        _menuRows.Add($"- {padded}{dur}");
                    }
                }
                _menuRows.Add("");
            }
            else
            {
                _menuRows.Add(_pageNames[_menuPage]);
                _menuRows.Add("");
                var effects = GetEffectsForPage(_menuPage);
                if (effects != null)
                {
                    int maxW = effects.Max(e => e.Name.Length);
                    foreach (var eff in effects)
                    {
                        bool on = _mod.Config.IsEffectEnabled(eff.Id);
                        string dur = FormatDuration(eff.DurationSeconds);
                        string label = eff.IsDaily ? "(Daily)" : (dur != "" ? dur : "");
                        string padded = PadNameForPage(eff.Name, maxW);
                        _menuRows.Add($"[{(on ? "x" : " ")}] {padded}{label}");
                    }
                }
                _menuRows.Add("");
            }

            _menuRows.Add("[Save & Close]");
        }

        void ToggleSelected()
        {
            int idx = _selectedRow;
            var c = _mod.Config;

            if (_menuPage == 0)
            {
                switch (idx)
                {
                    case 0: case 1: case 2: case 3: break;
                    case 4: c.Enabled = !c.Enabled; break;
                    case 5:
                        {
                            int[] intervals = { 15, 30, 60, 90, 120 };
                            int currentIdx = System.Array.IndexOf(intervals, c.IntervalSeconds);
                            if (currentIdx < 0) currentIdx = 2;
                            c.IntervalSeconds = intervals[(currentIdx + 1) % intervals.Length];
                            break;
                        }
                    case 6:
                        {
                            c.TwitchVotingEnabled = !c.TwitchVotingEnabled;
                            if (c.TwitchVotingEnabled) _mod.ConnectTwitch();
                            else _mod.DisconnectTwitch();
                            break;
                        }
                    case 7:
                        {
                            var def = _mod.Registry.PickRandom(c, _mod.IsHarmonyUsable(), new System.Random(), _mod.DailyManager?.ActiveDailyEffectId, _mod.DailyManager?.QueuedDailyEffectId);
                            _mod.Ticker.ManualTrigger(def);
                            break;
                        }
                    case 8: ToggleDifficultyCategory(c, Difficulty.Basic, false); break;
                    case 9: ToggleDifficultyCategory(c, Difficulty.Complex, false); break;
                    case 10: ToggleDifficultyCategory(c, Difficulty.Weird, false); break;
                    case 11: ToggleDailyEffects(c); break;
                    case 12: break;
                    default:
                        if (idx == _menuRows.Count - 1)
                        {
                            _mod.Helper.Data.WriteJsonFile("config.json", c);
                            _menuOpen = false;
                            return;
                        }
                        break;
                }
            }
            else if (_menuPage == 5)
            {
                if (idx == _menuRows.Count - 1)
                {
                    _mod.Helper.Data.WriteJsonFile("config.json", c);
                    _menuOpen = false;
                    return;
                }
            }
            else
            {
                if (idx == _menuRows.Count - 1)
                {
                    _mod.Helper.Data.WriteJsonFile("config.json", c);
                    _menuOpen = false;
                    return;
                }
                int effectIdx = idx - 2;
                var effects = GetEffectsForPage(_menuPage);
                if (effects != null && effectIdx >= 0 && effectIdx < effects.Count)
                {
                    var eff = effects[effectIdx];
                    bool current = c.IsEffectEnabled(eff.Id);
                    c.EffectOverrides[eff.Id] = !current;
                }
            }
            RebuildMenuRows();
        }

        void DrawMenu(SpriteBatch b)
        {
            int x = 40, y = 130;
            int rowH = 24;
            int titleRowH = 36;
            int spacerH = 24;
            var font = Game1.smallFont;
            var bigFont = Game1.dialogueFont ?? Game1.smallFont;

            int totalH = 0;
            for (int i = 0; i < _menuRows.Count; i++)
            {
                if (_menuPage == 0 && i == 0) totalH += titleRowH;
                else if (string.IsNullOrEmpty(_menuRows[i])) totalH += spacerH;
                else totalH += rowH;
            }

            float maxRowW = 0;
            for (int i = 0; i < _menuRows.Count; i++)
            {
                if (string.IsNullOrEmpty(_menuRows[i])) continue;
                var w = font.MeasureString(_menuRows[i]).X;
                if (w > maxRowW) maxRowW = w;
            }
            if (_menuPage == 0)
            {
                float titleW = bigFont.MeasureString(_menuRows[0]).X;
                if (titleW > maxRowW) maxRowW = titleW;
            }
            float pageLabelW = font.MeasureString($"Page: {_pageNames[_menuPage]} (Left/Right to switch)").X;
            float hintW = font.MeasureString("F7 toggle | Up/Down nav | Enter toggle | Q trigger | Left/Right pages | Esc close").X;
            float contentW = Math.Max(maxRowW, Math.Max(pageLabelW, hintW));
            int bgWidth = (int)contentW + 40;

            b.Draw(Game1.fadeToBlackRect, new Rectangle(x - 12, y - 32, bgWidth, totalH + 16 + 56), Color.Black * 0.7f);

            b.DrawString(font, $"Page: {_pageNames[_menuPage]} (Left/Right to switch)", new Vector2(x, y - 24), Color.Cyan);

            int curY = y;
            for (int i = 0; i < _menuRows.Count; i++)
            {
                if (string.IsNullOrEmpty(_menuRows[i]))
                {
                    curY += spacerH;
                    continue;
                }

                if (_menuPage == 0 && i == 0)
                {
                    b.DrawString(bigFont, _menuRows[i], new Vector2(x, curY), Color.Gold);
                    curY += titleRowH;
                }
                else if (_menuPage == 0 && (i == 1 || i == 2))
                {
                    b.DrawString(font, _menuRows[i], new Vector2(x, curY), Color.LightGray);
                    curY += rowH;
                }
                else if (_menuPage != 0 && i == 0)
                {
                    b.DrawString(font, _menuRows[i], new Vector2(x, curY), Color.Cyan);
                    curY += rowH;
                }
                else
                {
                    var color = i == _selectedRow ? Color.Yellow : Color.White;
                    b.DrawString(font, _menuRows[i], new Vector2(x, curY), color);
                    curY += rowH;
                }
            }

            b.DrawString(font, "F7 toggle | Up/Down nav | Enter toggle | Q trigger | Left/Right pages | Esc close", new Vector2(x, curY + 8), Color.LightGray);
        }

        void ToggleDailyEffects(ChaosConfig c)
        {
            bool newState = !AreDailyEffectsEnabled();
            foreach (var eff in _mod.Registry.All)
            {
                if (eff.IsDaily)
                    c.EffectOverrides[eff.Id] = newState;
            }
        }

        void ToggleDifficultyCategory(ChaosConfig c, Difficulty diff, bool dailyOnly)
        {
            bool newState = !c.IsDifficultyEnabled(diff);
            if (!dailyOnly)
                c.DifficultyFilter[diff] = newState;
            foreach (var eff in _mod.Registry.All)
            {
                if (dailyOnly)
                {
                    if (eff.IsDaily)
                        c.EffectOverrides[eff.Id] = newState;
                }
                else
                {
                    if (eff.Difficulty == diff && !eff.IsDaily)
                        c.EffectOverrides[eff.Id] = newState;
                }
            }
        }

        void TriggerSelectedEffect()
        {
            if (_menuPage == 0) return;
            int idx = _selectedRow;
            int effectIdx = idx - 2;
            var effects = GetEffectsForPage(_menuPage);
            if (effects != null && effectIdx >= 0 && effectIdx < effects.Count)
            {
                var eff = effects[effectIdx];
                if (!eff.RequiresHarmony || _mod.IsHarmonyUsable())
                {
                    _mod.Monitor.Log($"Manual trigger (Q key): {eff.Name} ({eff.Id})", StardewModdingAPI.LogLevel.Info);
                    _mod.Ticker.ManualTrigger(eff);
                }
            }
        }

        bool AreDailyEffectsEnabled()
        {
            foreach (var eff in _mod.Registry.All)
            {
                if (eff.IsDaily && !_mod.Config.IsEffectEnabled(eff.Id))
                    return false;
            }
            return true;
        }
    }
}