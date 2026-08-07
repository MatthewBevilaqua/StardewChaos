using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;

namespace StardewChaos
{
    public class HardcorePongMinigame : IMinigame
    {
        private const int ScreenW = 1280;
        private const int ScreenH = 720;

        private const int PaddleW = 16;
        private const int PaddleH = 90;
        private const int BallSize = 14;
        private const int PaddleMargin = 40;

        private const float PaddleSpeed = 11f;
        private const float CpuSpeed = 9f;
        private const float BallSpeed = 14f;
        private const float MaxBallSpeed = 24f;

        private Vector2 _ball;
        private Vector2 _ballVel;
        private float _leftY;
        private float _rightY;
        private int _scoreLeft;
        private int _scoreRight;

        private bool _upHeld;
        private bool _downHeld;
        private bool _gameOver;
        private bool _playerWon;
        private int _endTimer;
        private int _serveDelay = 60;

        private static readonly Random Rng = new();

        public HardcorePongMinigame()
        {
            _leftY = (ScreenH - PaddleH) / 2f;
            _rightY = (ScreenH - PaddleH) / 2f;
            ResetBall(serveTowardPlayer: Rng.Next(2) == 0);
        }

        private void ResetBall(bool serveTowardPlayer)
        {
            _ball = new Vector2(ScreenW / 2f - BallSize / 2f, ScreenH / 2f - BallSize / 2f);
            float angle = (float)(Rng.NextDouble() * 0.6 - 0.3);
            float dir = serveTowardPlayer ? -1f : 1f;
            _ballVel = new Vector2(dir * BallSpeed * (float)Math.Cos(angle), BallSpeed * (float)Math.Sin(angle));
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
                if (_upHeld) _leftY -= PaddleSpeed;
                if (_downHeld) _leftY += PaddleSpeed;
                _leftY = MathHelper.Clamp(_leftY, 0, ScreenH - PaddleH);
                return false;
            }

            if (_upHeld) _leftY -= PaddleSpeed;
            if (_downHeld) _leftY += PaddleSpeed;
            _leftY = MathHelper.Clamp(_leftY, 0, ScreenH - PaddleH);

            float ballCenterY = _ball.Y + BallSize / 2f;
            float cpuTarget = ballCenterY - PaddleH / 2f;
            if (_rightY < cpuTarget) _rightY += Math.Min(CpuSpeed, cpuTarget - _rightY);
            else if (_rightY > cpuTarget) _rightY -= Math.Min(CpuSpeed, _rightY - cpuTarget);
            _rightY = MathHelper.Clamp(_rightY, 0, ScreenH - PaddleH);

            _ball += _ballVel;

            if (_ball.Y <= 0)
            {
                _ball.Y = 0;
                _ballVel.Y = Math.Abs(_ballVel.Y);
                Game1.playSound("dwop");
            }
            else if (_ball.Y + BallSize >= ScreenH)
            {
                _ball.Y = ScreenH - BallSize;
                _ballVel.Y = -Math.Abs(_ballVel.Y);
                Game1.playSound("dwop");
            }

            Rectangle leftRect = new(PaddleMargin, (int)_leftY, PaddleW, PaddleH);
            Rectangle rightRect = new(ScreenW - PaddleMargin - PaddleW, (int)_rightY, PaddleW, PaddleH);
            Rectangle ballRect = new((int)_ball.X, (int)_ball.Y, BallSize, BallSize);

            if (ballRect.Intersects(leftRect) && _ballVel.X < 0)
            {
                _ball.X = leftRect.Right;
                _ballVel.X = Math.Abs(_ballVel.X) * 1.05f;
                float hit = (ballCenterY - (leftRect.Y + PaddleH / 2f)) / (PaddleH / 2f);
                _ballVel.Y = hit * BallSpeed;
                ClampBallSpeed();
                Game1.playSound("toolSwap");
            }
            else if (ballRect.Intersects(rightRect) && _ballVel.X > 0)
            {
                _ball.X = rightRect.Left - BallSize;
                _ballVel.X = -Math.Abs(_ballVel.X) * 1.05f;
                float hit = (ballCenterY - (rightRect.Y + PaddleH / 2f)) / (PaddleH / 2f);
                _ballVel.Y = hit * BallSpeed;
                ClampBallSpeed();
                Game1.playSound("toolSwap");
            }

            if (_ball.X < 0)
            {
                _scoreRight++;
                OnCpuScore();
            }
            else if (_ball.X + BallSize > ScreenW)
            {
                _scoreLeft++;
                OnPlayerScore();
            }

            return false;
        }

        private void ClampBallSpeed()
        {
            float speed = _ballVel.Length();
            if (speed > MaxBallSpeed)
            {
                _ballVel *= MaxBallSpeed / speed;
            }
        }

        private void OnPlayerScore()
        {
            _gameOver = true;
            _playerWon = true;
            Game1.playSound("yoba");
        }

        private void OnCpuScore()
        {
            _gameOver = true;
            _playerWon = false;
            AdvanceTime(500);
            try { Game1.playSound(ModEntry.LossSoundCueId); } catch { Game1.playSound("cancel"); }
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
                var vp = Game1.graphics.GraphicsDevice.Viewport;
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, vp.Width, vp.Height), Color.Black);

                Rectangle leftRect = new(PaddleMargin, (int)_leftY, PaddleW, PaddleH);
                Rectangle rightRect = new(ScreenW - PaddleMargin - PaddleW, (int)_rightY, PaddleW, PaddleH);
                Rectangle ballRect = new((int)_ball.X, (int)_ball.Y, BallSize, BallSize);

                DrawRect(b, leftRect, Color.White);
                DrawRect(b, rightRect, Color.White);
                DrawRect(b, ballRect, Color.Red);

                int halfX = ScreenW / 2;
                for (int y = 20; y < ScreenH - 20; y += 30)
                {
                    DrawRect(b, new Rectangle(halfX - 2, y, 4, 18), Color.DarkRed);
                }

                DrawCenteredText(b, $"HARDCORE PONG   {_scoreLeft}  -  {_scoreRight}", ScreenW / 2, 30, Color.Red);

                if (_serveDelay > 0)
                {
                    int seconds = (_serveDelay + 59) / 60;
                    DrawCenteredText(b, $"GET READY... {seconds}", ScreenW / 2, ScreenH / 2 - 20, Color.Yellow);
                    DrawCenteredText(b, "W / S  or  Up / Down", ScreenW / 2, ScreenH / 2 + 20, Color.Gray);
                }
                else if (!_gameOver)
                {
                    DrawCenteredText(b, "SCORE OR LOSE 8 HOURS", ScreenW / 2, ScreenH - 50, Color.LightGray);
                    DrawCenteredText(b, "W / S  or  Up / Down", ScreenW / 2, ScreenH - 25, Color.Gray);
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

        private static void DrawRect(SpriteBatch b, Rectangle rect, Color color)
        {
            b.Draw(Game1.staminaRect, rect, color);
        }

        private static void DrawCenteredText(SpriteBatch b, string text, int x, int y, Color color)
        {
            SpriteFont font = Game1.smallFont;
            Vector2 size = font.MeasureString(text);
            b.DrawString(font, text, new Vector2(x - size.X / 2f, y), color);
        }

        public void receiveKeyPress(Keys key)
        {
            if (key == Keys.W || key == Keys.Up) _upHeld = true;
            if (key == Keys.S || key == Keys.Down) _downHeld = true;
            if (key == Keys.Escape && !_gameOver)
            {
                _gameOver = true;
                _playerWon = false;
                AdvanceTime(500);
                try { Game1.playSound(ModEntry.LossSoundCueId); } catch { Game1.playSound("cancel"); }
            }
        }

        public void receiveKeyRelease(Keys key)
        {
            if (key == Keys.W || key == Keys.Up) _upHeld = false;
            if (key == Keys.S || key == Keys.Down) _downHeld = false;
        }

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
        public string minigameId() { return "HardcorePong"; }
    }
}