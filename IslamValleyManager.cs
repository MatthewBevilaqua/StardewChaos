using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace StardewChaos
{
    public class IslamValleyManager
    {
        private readonly ModEntry _mod;

        private enum State { Idle, PrayerPending, PrayerComplete }
        private State _state = State.Idle;

        private static readonly int[] PrayerTimes = { 1200, 1600, 1800, 2000 };
        private static readonly string[] PrayerNames = { "Dhuhr", "Asr", "Maghrib", "Isha" };
        private int _currentPrayerIndex = -1;

        private int _secondsRemaining = 0;
        private float _progress = 1f;

        private static readonly string TargetPhrase = "PRAISE ALLAH";
        private int _typedIndex = 0;

        internal bool IsActive { get; private set; } = false;
        internal string ActivePrayerName { get; private set; } = "";

        private SavedOutfit _savedOutfit;

        public IslamValleyManager(ModEntry mod)
        {
            _mod = mod;
        }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
            _state = State.Idle;
            _currentPrayerIndex = -1;
            _typedIndex = 0;
            ActivePrayerName = "";
            _mod.Monitor.Log("Islam Valley activated — applying outfit.", LogLevel.Info);
            ApplyOutfit();
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            _state = State.Idle;
            ActivePrayerName = "";
            _typedIndex = 0;
            _mod.Monitor.Log("Islam Valley deactivated — restoring outfit.", LogLevel.Info);
            RestoreOutfit();
        }

        public void OnDayStarted()
        {
            if (!IsActive) return;
            _state = State.Idle;
            _currentPrayerIndex = -1;
            ActivePrayerName = "";
            _typedIndex = 0;
        }

        public void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (!IsActive || _state != State.Idle) return;

            for (int i = 0; i < PrayerTimes.Length; i++)
            {
                if (e.NewTime == PrayerTimes[i])
                {
                    _currentPrayerIndex = i;
                    ActivePrayerName = PrayerNames[i];
                    _state = State.PrayerPending;
                    _secondsRemaining = 25;
                    _progress = 1f;
                    _typedIndex = 0;
                    _mod.Monitor.Log($"Prayer time: {PrayerNames[i]} ({e.NewTime}) — type PRAISE ALLAH.", LogLevel.Info);
                    Game1.playSound("questLog");
                    return;
                }
            }
        }

        public void OnOneSecondTick(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!IsActive || _state != State.PrayerPending) return;
            if (_mod.ShouldPauseTimers()) return;

            _secondsRemaining--;
            _progress = (float)_secondsRemaining / 25f;

            if (_secondsRemaining <= 0)
            {
                _mod.Monitor.Log($"Prayer failed ({ActivePrayerName}) — forcing day end.", LogLevel.Info);
                _state = State.Idle;
                ActivePrayerName = "";
                _typedIndex = 0;
                ForceEndDay();
            }
        }

        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!IsActive || _state != State.PrayerPending) return;

            char c = '\0';
            if (e.Button == SButton.A) c = 'A';
            else if (e.Button == SButton.B) c = 'B';
            else if (e.Button == SButton.C) c = 'C';
            else if (e.Button == SButton.D) c = 'D';
            else if (e.Button == SButton.E) c = 'E';
            else if (e.Button == SButton.F) c = 'F';
            else if (e.Button == SButton.G) c = 'G';
            else if (e.Button == SButton.H) c = 'H';
            else if (e.Button == SButton.I) c = 'I';
            else if (e.Button == SButton.J) c = 'J';
            else if (e.Button == SButton.K) c = 'K';
            else if (e.Button == SButton.L) c = 'L';
            else if (e.Button == SButton.M) c = 'M';
            else if (e.Button == SButton.N) c = 'N';
            else if (e.Button == SButton.O) c = 'O';
            else if (e.Button == SButton.P) c = 'P';
            else if (e.Button == SButton.Q) c = 'Q';
            else if (e.Button == SButton.R) c = 'R';
            else if (e.Button == SButton.S) c = 'S';
            else if (e.Button == SButton.T) c = 'T';
            else if (e.Button == SButton.U) c = 'U';
            else if (e.Button == SButton.V) c = 'V';
            else if (e.Button == SButton.W) c = 'W';
            else if (e.Button == SButton.X) c = 'X';
            else if (e.Button == SButton.Y) c = 'Y';
            else if (e.Button == SButton.Z) c = 'Z';
            else if (e.Button == SButton.Space) c = ' ';

            if (c != '\0')
            {
                char expected = TargetPhrase[_typedIndex];
                if (char.ToUpperInvariant(c) == expected)
                {
                    _typedIndex++;
                    _mod.Helper.Input.Suppress(e.Button);
                    Game1.playSound("toolSwap");

                    if (_typedIndex >= TargetPhrase.Length)
                    {
                        _mod.Monitor.Log($"Prayer completed ({ActivePrayerName}).", LogLevel.Info);
                        _state = State.PrayerComplete;
                        ActivePrayerName = "";
                        _typedIndex = 0;
                        Game1.playSound("yoba");
                    }
                }
                else
                {
                    _mod.Helper.Input.Suppress(e.Button);
                }
            }
        }

        public void OnRendered(object sender, RenderedEventArgs e)
        {
            if (!IsActive || _state != State.PrayerPending) return;

            var b = e.SpriteBatch;
            var vp = Game1.graphics.GraphicsDevice.Viewport;
            int screenW = vp.Width;
            int screenH = vp.Height;

            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, screenW, screenH), Color.Black * 0.6f);

            var bigFont = Game1.dialogueFont ?? Game1.smallFont;
            var smallFont = Game1.smallFont;

            string header = "PRAY to Allah";
            string instr = "Type: PRAISE ALLAH";
            string prayerLabel = $"({ActivePrayerName})";

            var headerSize = bigFont.MeasureString(header);
            DrawBoldString(b, bigFont, header, new Vector2(screenW / 2f - headerSize.X / 2f, screenH / 2f - 120), Color.White);

            var instrSize = smallFont.MeasureString(instr);
            DrawBoldString(b, smallFont, instr, new Vector2(screenW / 2f - instrSize.X / 2f, screenH / 2f - 60), Color.LightGray);

            DrawPhrase(b, smallFont, screenW, screenH);

            var labelSize = smallFont.MeasureString(prayerLabel);
            DrawBoldString(b, smallFont, prayerLabel, new Vector2(screenW / 2f - labelSize.X / 2f, screenH / 2f + 80), Color.LightGray);

            int barW = 400;
            int barH = 24;
            int barX = (int)(screenW / 2f - barW / 2f);
            int barY = screenH / 2 + 110;
            int filled = (int)(barW * _progress);

            b.Draw(Game1.fadeToBlackRect, new Rectangle(barX, barY, barW, barH), Color.Black * 0.6f);
            b.Draw(Game1.staminaRect, new Rectangle(barX, barY, filled, barH), Color.OrangeRed);
            b.Draw(Game1.staminaRect, new Rectangle(barX, barY, barW, 2), Color.White * 0.6f);
            b.Draw(Game1.staminaRect, new Rectangle(barX, barY + barH - 2, barW, 2), Color.White * 0.6f);
            b.Draw(Game1.staminaRect, new Rectangle(barX, barY, 2, barH), Color.White * 0.6f);
            b.Draw(Game1.staminaRect, new Rectangle(barX + barW - 2, barY, 2, barH), Color.White * 0.6f);
        }

        private void DrawPhrase(SpriteBatch b, SpriteFont font, int screenW, int screenH)
        {
            float charWidth = font.MeasureString("P").X;
            float spacing = 2f;
            float totalWidth = TargetPhrase.Length * (charWidth + spacing) - spacing;
            float startX = screenW / 2f - totalWidth / 2f;
            float y = screenH / 2f - 10;

            for (int i = 0; i < TargetPhrase.Length; i++)
            {
                string ch = TargetPhrase[i].ToString();
                var chSize = font.MeasureString(ch);
                float x = startX + i * (charWidth + spacing);
                Color color = i < _typedIndex ? Color.LimeGreen : Color.White;

                if (i < _typedIndex)
                {
                    b.DrawString(font, ch, new Vector2(x + 1, y + 1), Color.Black * 0.5f);
                }
                b.DrawString(font, ch, new Vector2(x, y), color);

                b.Draw(Game1.staminaRect, new Rectangle((int)x, (int)(y + chSize.Y + 4), (int)charWidth, 2),
                    i < _typedIndex ? Color.LimeGreen * 0.7f : Color.White * 0.3f);
            }
        }

        private void ForceEndDay()
        {
            try
            {
                Game1.player.startToPassOut();
                Game1.player.freezePause = 7000;
            }
            catch (Exception ex)
            {
                _mod.Monitor.Log($"ForceEndDay failed: {ex.Message}", LogLevel.Error);
            }
        }

        private void ApplyOutfit()
        {
            try
            {
                var p = Game1.player;
                _savedOutfit = new SavedOutfit
                {
                    HatIndex = -1,
                    ShirtId = p.shirt.Value,
                    PantsId = p.pants.Value,
                    PantsColor = p.pantsColor.Value,
                    ShirtColor = p.shirtItem.Value?.clothesColor.Value ?? Color.White
                };
                if (p.hat.Value != null && int.TryParse(p.hat.Value.ItemId, out int hatIdx))
                    _savedOutfit.HatIndex = hatIdx;

                p.changeHat(65);

                string shirtId = (int)p.netGender.Value == 0 ? "1176" : "1177";
                var shirt = new Clothing(shirtId);
                p.Equip(shirt, p.shirtItem);
                p.shirt.Set("-1");
                if (p.shirtItem.Value != null && p.shirtItem.Value.dyeable.Value)
                    p.shirtItem.Value.Dye(Color.Black, 1f);
                p.FarmerRenderer.MarkSpriteDirty();

                p.changePantStyle("11");
                p.changePantsColor(Color.Black);
            }
            catch (Exception e)
            {
                _mod.Monitor.Log($"ApplyOutfit failed: {e.Message}", LogLevel.Error);
            }
        }

        private void RestoreOutfit()
        {
            try
            {
                var p = Game1.player;
                p.changePantStyle(_savedOutfit.PantsId);
                p.changePantsColor(_savedOutfit.PantsColor);
                if (_savedOutfit.ShirtId != null && _savedOutfit.ShirtId != "-1")
                {
                    p.changeShirt(_savedOutfit.ShirtId);
                }
                else
                {
                    p.shirt.Set("-1");
                    if (p.shirtItem.Value != null)
                        p.shirtItem.Value.clothesColor.Value = _savedOutfit.ShirtColor;
                }
                p.FarmerRenderer.MarkSpriteDirty();
                p.changeHat(_savedOutfit.HatIndex);
            }
            catch (Exception e)
            {
                _mod.Monitor.Log($"RestoreOutfit failed: {e.Message}", LogLevel.Error);
            }
        }

        private static void DrawBoldString(SpriteBatch b, SpriteFont font, string text, Vector2 pos, Color color)
        {
            b.DrawString(font, text, pos + new Vector2(2, 2), Color.Black * 0.5f);
            b.DrawString(font, text, pos + new Vector2(1, 0), color);
            b.DrawString(font, text, pos + new Vector2(-1, 0), color);
            b.DrawString(font, text, pos, color);
        }

        private class SavedOutfit
        {
            public int HatIndex;
            public string ShirtId;
            public string PantsId;
            public Color PantsColor;
            public Color ShirtColor;
        }
    }
}