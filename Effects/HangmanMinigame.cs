using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;

namespace StardewChaos
{
    public class HangmanMinigame : IMinigame
    {
        private const int ScreenW = 1280;
        private const int ScreenH = 720;
        private const int MaxWrong = 6;

        private static readonly string[] Words =
        {
            "ABIGAIL","SEBASTIAN","MARU","PENNY","HARVEY","SHANE","EMILY","HALEY",
            "ELLIOTT","LEAH","PIERRE","ROBIN","DEMETRIUS","CAROLINE","MARNIE","LINUS",
            "WILLY","SANDY","KROBUS","LEO","WIZARD","DWARF","LEWIS","JODI",
            "PARSNIP","CAULIFLOWER","PUMPKIN","STARFRUIT","MELON","IRIDIUM","DIAMOND",
            "EMERALD","MAYONNAISE","WINE","CHEESE","HONEY","MAPLE","OAK","PINE",
            "CHICKEN","RABBIT","DINOSAUR","OSTRICH","TURTLE","LLAMA","HORSE","PIG",
            "WATERFALL","MOUNTAIN","FOREST","RIVER","OCEAN","VALLEY","THUNDER","RAINBOW",
            "SUNSET","SPRING","SUMMER","AUTUMN","WINTER","LIGHTNING",
            "HARVEST","SCARECROW","SPRINKLER","FERTILIZER","GREENHOUSE","SILO","BARN","COOP",
            "FENCE","BRIDGE","TRACTOR",
            "PIZZA","COOKIE","CAKE","SOUP","SALAD","SPAGHETTI","PANCAKES","COFFEE",
            "SANDWACH","STEAK","ICECREAM",
            "JUNIMO","GRANGE","FESTIVAL","CARNIVAL","MUSEUM","LIBRARY","SALOON","MINES",
            "QUARRY","BEACH","DESERT","ISLAND","VOLCANO","SHED","CELLAR",
        };

        private static readonly Random Rng = new();

        private string _word;
        private readonly HashSet<char> _guessed = new();
        private int _wrongCount;
        private bool _gameOver;
        private bool _playerWon;
        private int _endTimer;
        private int _serveDelay = 60;

        private static HangmanMinigame _instance;

        public HangmanMinigame()
        {
            _word = Words[Rng.Next(Words.Length)];
            _wrongCount = 0;
            _instance = this;
        }

        public static void OnButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (_instance == null || _instance._gameOver || _instance._serveDelay > 0) return;
            char c = '\0';
            if (e.Button == StardewModdingAPI.SButton.A) c = 'A';
            else if (e.Button == StardewModdingAPI.SButton.B) c = 'B';
            else if (e.Button == StardewModdingAPI.SButton.C) c = 'C';
            else if (e.Button == StardewModdingAPI.SButton.D) c = 'D';
            else if (e.Button == StardewModdingAPI.SButton.E) c = 'E';
            else if (e.Button == StardewModdingAPI.SButton.F) c = 'F';
            else if (e.Button == StardewModdingAPI.SButton.G) c = 'G';
            else if (e.Button == StardewModdingAPI.SButton.H) c = 'H';
            else if (e.Button == StardewModdingAPI.SButton.I) c = 'I';
            else if (e.Button == StardewModdingAPI.SButton.J) c = 'J';
            else if (e.Button == StardewModdingAPI.SButton.K) c = 'K';
            else if (e.Button == StardewModdingAPI.SButton.L) c = 'L';
            else if (e.Button == StardewModdingAPI.SButton.M) c = 'M';
            else if (e.Button == StardewModdingAPI.SButton.N) c = 'N';
            else if (e.Button == StardewModdingAPI.SButton.O) c = 'O';
            else if (e.Button == StardewModdingAPI.SButton.P) c = 'P';
            else if (e.Button == StardewModdingAPI.SButton.Q) c = 'Q';
            else if (e.Button == StardewModdingAPI.SButton.R) c = 'R';
            else if (e.Button == StardewModdingAPI.SButton.S) c = 'S';
            else if (e.Button == StardewModdingAPI.SButton.T) c = 'T';
            else if (e.Button == StardewModdingAPI.SButton.U) c = 'U';
            else if (e.Button == StardewModdingAPI.SButton.V) c = 'V';
            else if (e.Button == StardewModdingAPI.SButton.W) c = 'W';
            else if (e.Button == StardewModdingAPI.SButton.X) c = 'X';
            else if (e.Button == StardewModdingAPI.SButton.Y) c = 'Y';
            else if (e.Button == StardewModdingAPI.SButton.Z) c = 'Z';
            if (c != '\0')
            {
                _instance.GuessLetter(c);
                ModEntry.Instance.Helper.Input.Suppress(e.Button);
            }
        }

        public bool tick(GameTime time)
        {
            if (_gameOver)
            {
                _endTimer++;
                return _endTimer >= 120;
            }
            if (_serveDelay > 0)
            {
                _serveDelay--;
                return false;
            }
            return false;
        }

        private void GuessLetter(char c)
        {
            if (_gameOver) return;
            c = char.ToUpperInvariant(c);
            if (!char.IsLetter(c)) return;
            if (_guessed.Contains(c)) return;

            _guessed.Add(c);

            if (_word.Contains(c))
            {
                Game1.playSound("toolSwap");
                bool allRevealed = true;
                foreach (char wc in _word)
                {
                    if (!_guessed.Contains(wc)) { allRevealed = false; break; }
                }
                if (allRevealed)
                {
                    _gameOver = true;
                    _playerWon = true;
                    Game1.playSound("yoba");
                }
            }
            else
            {
                _wrongCount++;
                Game1.playSound("dwop");
                if (_wrongCount >= MaxWrong)
                {
                    _gameOver = true;
                    _playerWon = false;
                    AdvanceTime(300);
                    try { Game1.playSound(ModEntry.LossSoundCueId); } catch { Game1.playSound("cancel"); }
                }
            }
        }

        private static void AdvanceTime(int minutes)
        {
            int newTime = Game1.timeOfDay + minutes;
            while (newTime >= 2400) newTime -= 2400;
            Game1.timeOfDay = newTime;
        }

        public void draw(SpriteBatch b)
        {
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            try
            {
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, ScreenW, ScreenH), Color.Black);

                DrawGallows(b, _wrongCount);

                DrawWord(b);

                DrawAlphabet(b);

                DrawCenteredText(b, "HANGMAN", ScreenW / 2, 30, Color.White);

                if (_serveDelay > 0)
                {
                    DrawCenteredText(b, "GET READY...", ScreenW / 2, ScreenH / 2, Color.Yellow);
                }
                else if (_gameOver)
                {
                    if (_playerWon)
                    {
                        DrawCenteredText(b, "YOU WIN!", ScreenW / 2, ScreenH / 2 + 60, Color.LimeGreen);
                    }
                    else
                    {
                        DrawCenteredText(b, $"WORD: {_word}", ScreenW / 2, ScreenH / 2 + 40, Color.Yellow);
                        DrawCenteredText(b, "YOU LOSE  -  +3 HOURS", ScreenW / 2, ScreenH / 2 + 80, Color.Red);
                    }
                }
                else
                {
                    var bigFont = Game1.dialogueFont ?? Game1.smallFont;
                    string wrongText = $"Wrong: {_wrongCount}/{MaxWrong}";
                    var wSize = bigFont.MeasureString(wrongText);
                    b.DrawString(bigFont, wrongText, new Vector2(ScreenW / 2 - wSize.X / 2f + 2, 380 + 2), Color.Black * 0.5f);
                    b.DrawString(bigFont, wrongText, new Vector2(ScreenW / 2 - wSize.X / 2f, 380), _wrongCount >= MaxWrong - 2 ? Color.Red : Color.White);
                    DrawCenteredText(b, "Type A-Z to guess  |  Esc to forfeit", ScreenW / 2, ScreenH - 25, Color.Gray);
                }
            }
            finally
            {
                b.End();
            }
        }

        private void DrawGallows(SpriteBatch b, int stage)
        {
            int gx = ScreenW / 2 - 200;
            int gy = 120;
            Color wood = new Color(139, 90, 43);
            Color body = new Color(220, 200, 180);

            b.Draw(Game1.staminaRect, new Rectangle(gx, gy + 240, 160, 12), wood);
            b.Draw(Game1.staminaRect, new Rectangle(gx + 10, gy, 12, 240), wood);
            b.Draw(Game1.staminaRect, new Rectangle(gx + 10, gy, 120, 12), wood);
            b.Draw(Game1.staminaRect, new Rectangle(gx + 128, gy, 8, 30), wood);

            if (stage >= 1) b.Draw(Game1.staminaRect, new Rectangle(gx + 120, gy + 30, 24, 24), body);
            if (stage >= 2) b.Draw(Game1.staminaRect, new Rectangle(gx + 128, gy + 54, 8, 60), body);
            if (stage >= 3) b.Draw(Game1.staminaRect, new Rectangle(gx + 108, gy + 60, 20, 6), body);
            if (stage >= 4) b.Draw(Game1.staminaRect, new Rectangle(gx + 128, gy + 60, 20, 6), body);
            if (stage >= 5) b.Draw(Game1.staminaRect, new Rectangle(gx + 110, gy + 114, 18, 6), body);
            if (stage >= 6) b.Draw(Game1.staminaRect, new Rectangle(gx + 128, gy + 114, 18, 6), body);
        }

        private void DrawWord(SpriteBatch b)
        {
            var font = Game1.dialogueFont ?? Game1.smallFont;
            int y = 420;
            int spacing = 50;
            int totalW = _word.Length * spacing;
            int startX = ScreenW / 2 - totalW / 2;

            for (int i = 0; i < _word.Length; i++)
            {
                char c = _word[i];
                int x = startX + i * spacing;
                b.Draw(Game1.staminaRect, new Rectangle(x, y + 40, spacing - 8, 4), Color.White * 0.6f);

                if (_guessed.Contains(c) || _gameOver)
                {
                    string letter = c.ToString();
                    var size = font.MeasureString(letter);
                    b.DrawString(font, letter, new Vector2(x + (spacing - 8) / 2f - size.X / 2f, y), Color.White);
                }
                else
                {
                    b.DrawString(font, "_", new Vector2(x + (spacing - 8) / 2f - 6, y), Color.White * 0.5f);
                }
            }
        }

        private void DrawAlphabet(SpriteBatch b)
        {
            var font = Game1.smallFont;
            int y = 560;
            int letterW = 40;
            int letterH = 32;
            int cols = 13;
            int totalW = cols * letterW;
            int startX = ScreenW / 2 - totalW / 2;

            for (int i = 0; i < 26; i++)
            {
                char c = (char)('A' + i);
                int col = i % cols;
                int row = i / cols;
                int x = startX + col * letterW;
                int yy = y + row * (letterH + 4);

                bool guessed = _guessed.Contains(c);
                bool inWord = guessed && _word.Contains(c);

                Color bg = guessed ? (inWord ? new Color(40, 80, 40) : new Color(80, 30, 30)) : new Color(30, 30, 30);
                Color fg = guessed ? (inWord ? Color.LimeGreen : Color.Red) : Color.White;

                b.Draw(Game1.staminaRect, new Rectangle(x, yy, letterW - 4, letterH), bg);
                b.Draw(Game1.staminaRect, new Rectangle(x, yy, letterW - 4, 2), Color.White * 0.3f);
                b.Draw(Game1.staminaRect, new Rectangle(x, yy + letterH - 2, letterW - 4, 2), Color.White * 0.3f);

                string letter = c.ToString();
                var size = font.MeasureString(letter);
                b.DrawString(font, letter, new Vector2(x + (letterW - 4) / 2f - size.X / 2f, yy + (letterH - size.Y) / 2f), fg);
            }
        }

        private static void DrawCenteredText(SpriteBatch b, string text, int x, int y, Color color)
        {
            SpriteFont font = Game1.smallFont;
            Vector2 size = font.MeasureString(text);
            b.DrawString(font, text, new Vector2(x - size.X / 2f, y), color);
        }

        public void receiveKeyPress(Keys key)
        {
            if (_serveDelay > 0) return;
            if (key == Keys.Escape && !_gameOver)
            {
                _gameOver = true;
                _playerWon = false;
                AdvanceTime(300);
                try { Game1.playSound(ModEntry.LossSoundCueId); } catch { Game1.playSound("cancel"); }
                return;
            }

            if (_gameOver) return;

            if (key >= Keys.A && key <= Keys.Z)
            {
                char c = (char)('A' + (key - Keys.A));
                GuessLetter(c);
            }
        }

        public void receiveKeyRelease(Keys key) { }
        public void receiveLeftClick(int x, int y, bool playSound = true) { }
        public void releaseLeftClick(int x, int y) { }
        public void receiveRightClick(int x, int y, bool playSound = true) { }
        public void releaseRightClick(int x, int y) { }
        public void leftClickHeld(int x, int y) { }
        public void rightClickHeld(int x, int y) { }
        public bool forceQuit() { return false; }
        public void changeGameState(int which) { }
        public bool doMainGameUpdates() { return false; }
        public bool overrideFreeMouseMovement() { return true; }
        public void changeScreenSize() { }
        public void unload() { }
        public void receiveEventPoke(int data) { }
        public string minigameId() { return "Hangman"; }
    }
}