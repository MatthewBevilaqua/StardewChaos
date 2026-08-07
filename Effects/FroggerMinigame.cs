using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;

namespace StardewChaos
{
    public class FroggerMinigame : IMinigame
    {
        private const int ScreenW = 1280;
        private const int ScreenH = 720;
        private const int TileSize = 80;
        private const int FrogSize = 28;

        private const int GoalZoneH = 80;
        private const int RiverStart = 80;
        private const int RiverLaneH = 80;
        private const int RiverLanes = 3;
        private const int MedianY = 320;
        private const int MedianH = 80;
        private const int RoadStart = 400;
        private const int RoadLaneH = 80;
        private const int RoadLanes = 3;
        private const int StartZoneY = 640;

        private Vector2 _frog;
        private int _frogGridY;
        private bool _onLog;
        private float _logDx;
        private bool _gameOver;
        private bool _playerWon;
        private int _endTimer;
        private int _serveDelay = 120;
        private int _timeLimit;
        private readonly float _speedMultiplier;
        private readonly int _homeCount;
        private int _lives;
        private readonly bool[] _homes;

        private static readonly Random Rng = new();

        private class Vehicle
        {
            public float X, Y;
            public float Speed;
            public int W, H;
            public Color Color;
        }

        private class Log
        {
            public float X, Y;
            public float Speed;
            public int W, H;
        }

        private readonly List<Vehicle> _vehicles = new();
        private readonly List<Log> _logs = new();
        private readonly int _timePenalty;

        public FroggerMinigame() : this(1f, 1, 2, 30f) { }

        public FroggerMinigame(float speedMultiplier, int homeCount, int lives, float timerSeconds, int timePenalty = 300)
        {
            _speedMultiplier = speedMultiplier;
            _homeCount = homeCount;
            _lives = lives;
            _timePenalty = timePenalty;
            _timeLimit = (int)(timerSeconds * 60);
            _homes = new bool[_homeCount];
            _frog = new Vector2(ScreenW / 2f - FrogSize / 2f, StartZoneY + 20);
            _frogGridY = 0;

            InitVehicles();
            InitLogs();
        }

        private void InitVehicles()
        {
            Color[] carColors = { Color.Red, Color.Blue, Color.Yellow, Color.Orange, Color.Magenta };
            for (int lane = 0; lane < RoadLanes; lane++)
            {
                float y = RoadStart + lane * RoadLaneH + (RoadLaneH - 32) / 2f;
                float speed = (lane % 2 == 0 ? 1f : -1f) * (2.5f + lane * 0.8f) * _speedMultiplier;
                int count = 4;
                float spacing = ScreenW / count;
                for (int i = 0; i < count; i++)
                {
                    _vehicles.Add(new Vehicle
                    {
                        X = i * spacing,
                        Y = y,
                        Speed = speed,
                        W = 52,
                        H = 32,
                        Color = carColors[(lane + i) % carColors.Length]
                    });
                }
            }
        }

        private void InitLogs()
        {
            for (int lane = 0; lane < RiverLanes; lane++)
            {
                float y = RiverStart + lane * RiverLaneH + (RiverLaneH - 28) / 2f;
                float speed = (lane % 2 == 0 ? -1f : 1f) * (1.5f + lane * 0.7f) * _speedMultiplier;
                int count = 3;
                float spacing = ScreenW / count;
                for (int i = 0; i < count; i++)
                {
                    _logs.Add(new Log
                    {
                        X = i * spacing,
                        Y = y,
                        Speed = speed,
                        W = 100,
                        H = 28
                    });
                }
            }
        }

        public bool tick(GameTime time)
        {
            if (_gameOver)
            {
                _endTimer++;
                return _endTimer >= 90;
            }

            if (_serveDelay > 0)
            {
                _serveDelay--;
                return false;
            }

            _timeLimit--;
            if (_timeLimit <= 0)
            {
                OnDeath();
                return false;
            }

            foreach (var v in _vehicles)
            {
                v.X += v.Speed;
                if (v.Speed > 0 && v.X > ScreenW) v.X = -v.W;
                if (v.Speed < 0 && v.X < -v.W) v.X = ScreenW;
            }

            foreach (var log in _logs)
            {
                log.X += log.Speed;
                if (log.Speed > 0 && log.X > ScreenW) log.X = -log.W;
                if (log.Speed < 0 && log.X < -log.W) log.X = ScreenW;
            }

            if (_onLog)
            {
                _frog.X += _logDx;
                if (_frog.X < 0 || _frog.X > ScreenW - FrogSize)
                {
                    OnDeath();
                    return false;
                }
                _onLog = false;
            }

            var frogRect = new Rectangle((int)_frog.X, (int)_frog.Y, FrogSize, FrogSize);

            if (_frog.Y >= RoadStart && _frog.Y < RoadStart + RoadLanes * RoadLaneH)
            {
                foreach (var v in _vehicles)
                {
                    var vRect = new Rectangle((int)v.X, (int)v.Y, v.W, v.H);
                    if (frogRect.Intersects(vRect))
                    {
                        OnDeath();
                        return false;
                    }
                }
            }

            if (_frog.Y >= RiverStart && _frog.Y < RiverStart + RiverLanes * RiverLaneH)
            {
                bool foundLog = false;
                foreach (var log in _logs)
                {
                    var logRect = new Rectangle((int)log.X, (int)log.Y, log.W, log.H);
                    if (frogRect.Intersects(logRect))
                    {
                        foundLog = true;
                        _onLog = true;
                        _logDx = log.Speed;
                        break;
                    }
                }
                if (!foundLog)
                {
                    OnDeath();
                    return false;
                }
            }

            if (_frog.Y < GoalZoneH)
            {
                int homeIdx = _homeCount > 1 ? (int)(_frog.X / (ScreenW / _homeCount)) : 0;
                if (homeIdx >= 0 && homeIdx < _homeCount && !_homes[homeIdx])
                {
                    _homes[homeIdx] = true;
                    int filled = 0;
                    foreach (var h in _homes) if (h) filled++;
                    if (filled >= _homeCount)
                    {
                        OnWin();
                        return false;
                    }
                    _frog = new Vector2(ScreenW / 2f - FrogSize / 2f, StartZoneY + 20);
                    _frogGridY = 0;
                    _onLog = false;
                    Game1.playSound("yoba");
                }
                else
                {
                    OnDeath();
                    return false;
                }
            }

            return false;
        }

        private void OnDeath()
        {
            _lives--;
            if (_lives <= 0)
            {
                _gameOver = true;
                _playerWon = false;
                AdvanceTime(_timePenalty);
                try { Game1.playSound(ModEntry.LossSoundCueId); } catch { Game1.playSound("cancel"); }
            }
            else
            {
                _frog = new Vector2(ScreenW / 2f - FrogSize / 2f, StartZoneY + 20);
                _frogGridY = 0;
                _onLog = false;
                Game1.playSound("cancel");
            }
        }

        private void OnWin()
        {
            _gameOver = true;
            _playerWon = true;
            Game1.playSound("yoba");
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

                b.Draw(Game1.staminaRect, new Rectangle(0, 0, ScreenW, GoalZoneH), new Color(20, 60, 20));
                int homeW = ScreenW / _homeCount;
                for (int i = 0; i < _homeCount; i++)
                {
                    Color hc = _homes[i] ? Color.LimeGreen : new Color(40, 80, 40);
                    b.Draw(Game1.staminaRect, new Rectangle(i * homeW + 40, 20, homeW - 80, 40), hc);
                }

                b.Draw(Game1.staminaRect, new Rectangle(0, RiverStart, ScreenW, RiverLanes * RiverLaneH), new Color(20, 40, 100));
                foreach (var log in _logs)
                {
                    b.Draw(Game1.staminaRect, new Rectangle((int)log.X, (int)log.Y, log.W, log.H), new Color(100, 60, 30));
                    b.Draw(Game1.staminaRect, new Rectangle((int)log.X, (int)log.Y, log.W, 2), new Color(140, 90, 50));
                }

                b.Draw(Game1.staminaRect, new Rectangle(0, MedianY, ScreenW, MedianH), new Color(60, 60, 60));

                b.Draw(Game1.staminaRect, new Rectangle(0, RoadStart, ScreenW, RoadLanes * RoadLaneH), new Color(50, 50, 50));
                foreach (var v in _vehicles)
                {
                    var vRect = new Rectangle((int)v.X, (int)v.Y, v.W, v.H);
                    b.Draw(Game1.staminaRect, vRect, v.Color);
                    b.Draw(Game1.staminaRect, new Rectangle((int)v.X, (int)v.Y, v.W, 2), Color.White * 0.3f);
                    b.Draw(Game1.staminaRect, new Rectangle((int)v.X, (int)v.Y + v.H - 2, v.W, 2), Color.White * 0.3f);
                }

                b.Draw(Game1.staminaRect, new Rectangle(0, StartZoneY, ScreenW, ScreenH - StartZoneY), new Color(20, 80, 20));

                var frogRect = new Rectangle((int)_frog.X, (int)_frog.Y, FrogSize, FrogSize);
                b.Draw(Game1.staminaRect, frogRect, Color.LimeGreen);
                b.Draw(Game1.staminaRect, new Rectangle((int)_frog.X + 4, (int)_frog.Y + 4, 6, 6), Color.White);
                b.Draw(Game1.staminaRect, new Rectangle((int)_frog.X + FrogSize - 10, (int)_frog.Y + 4, 6, 6), Color.White);

                DrawCenteredText(b, "FROGGER", ScreenW / 2, 5, Color.White);

                if (_serveDelay > 0)
                {
                    int seconds = (_serveDelay + 59) / 60;
                    DrawCenteredText(b, $"GET READY... {seconds}", ScreenW / 2, ScreenH / 2 - 20, Color.Yellow);
                    DrawCenteredText(b, "Arrows or WASD to hop", ScreenW / 2, ScreenH / 2 + 20, Color.Gray);
                }
                else if (!_gameOver)
                {
                    int seconds = _timeLimit / 60;
                    DrawCenteredText(b, $"TIME: {seconds}s  |  LIVES: {_lives}  |  HOMES: {_homeCount}", ScreenW / 2, ScreenH - 25, Color.LightGray);
                }
                else if (_playerWon)
                {
                    DrawCenteredText(b, "YOU WIN!", ScreenW / 2, ScreenH / 2, Color.LimeGreen);
                }
                else
                {
                    DrawCenteredText(b, "YOU LOSE  -  +8 HOURS", ScreenW / 2, ScreenH / 2, Color.Red);
                }
            }
            finally
            {
                b.End();
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
            if (_gameOver || _serveDelay > 0) return;

            float newX = _frog.X;
            float newY = _frog.Y;

            if (key == Keys.Up || key == Keys.W)
            {
                newY -= TileSize;
                _frogGridY++;
            }
            else if (key == Keys.Down || key == Keys.S)
            {
                newY += TileSize;
                _frogGridY = Math.Max(0, _frogGridY - 1);
            }
            else if (key == Keys.Left || key == Keys.A)
            {
                newX -= TileSize;
            }
            else if (key == Keys.Right || key == Keys.D)
            {
                newX += TileSize;
            }
            else if (key == Keys.Escape)
            {
                OnDeath();
                return;
            }
            else return;

            newX = MathHelper.Clamp(newX, 0, ScreenW - FrogSize);
            newY = MathHelper.Clamp(newY, 0, ScreenH - FrogSize);
            _frog = new Vector2(newX, newY);
            _onLog = false;
            Game1.playSound("thudStep");
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
        public bool overrideFreeMouseMovement() { return false; }
        public void changeScreenSize() { }
        public void unload() { }
        public void receiveEventPoke(int data) { }
        public string minigameId() => _homeCount > 1 ? "HardFrogger" : "Frogger";
    }
}