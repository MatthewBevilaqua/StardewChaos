using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;

namespace StardewChaos
{
    public class TetrisMinigame : IMinigame
    {
        private const int BoardW = 10;
        private const int BoardH = 20;
        private const int CellSize = 32;
        private const int ScreenW = 1280;
        private const int ScreenH = 720;

        private const int BoardX = 320;
        private const int BoardY = 60;

        private static readonly Color[] PieceColors = {
            Color.Cyan,       // I
            Color.Yellow,     // O
            Color.Orange,     // L
            Color.Blue,       // J
            Color.Purple,     // T
            Color.Green,      // S
            Color.Red         // Z
        };

        private static readonly int[,,] Rotations = {
            // I (type 0) - 2 rotations
            { {0,0,0,0},{1,1,1,1},{0,0,0,0},{0,0,0,0} },
            { {0,1,0,0},{0,1,0,0},{0,1,0,0},{0,1,0,0} },
            // O (type 1) - 1 rotation (duplicated)
            { {1,1,0,0},{1,1,0,0},{0,0,0,0},{0,0,0,0} },
            { {1,1,0,0},{1,1,0,0},{0,0,0,0},{0,0,0,0} },
            // L (type 2) - 4 rotations
            { {1,1,1,0},{1,0,0,0},{0,0,0,0},{0,0,0,0} },
            { {1,0,0,0},{1,0,0,0},{1,1,0,0},{0,0,0,0} },
            { {0,0,1,0},{1,1,1,0},{0,0,0,0},{0,0,0,0} },
            { {1,1,0,0},{0,1,0,0},{0,1,0,0},{0,0,0,0} },
            // J (type 3) - 4 rotations
            { {1,1,1,0},{0,0,1,0},{0,0,0,0},{0,0,0,0} },
            { {1,1,0,0},{1,0,0,0},{1,0,0,0},{0,0,0,0} },
            { {1,0,0,0},{1,1,1,0},{0,0,0,0},{0,0,0,0} },
            { {0,1,0,0},{0,1,0,0},{1,1,0,0},{0,0,0,0} },
            // T (type 4) - 4 rotations
            { {1,1,1,0},{0,1,0,0},{0,0,0,0},{0,0,0,0} },
            { {1,0,0,0},{1,1,0,0},{1,0,0,0},{0,0,0,0} },
            { {0,1,0,0},{1,1,1,0},{0,0,0,0},{0,0,0,0} },
            { {0,1,0,0},{1,1,0,0},{0,1,0,0},{0,0,0,0} },
            // S (type 5) - 2 rotations
            { {0,1,1,0},{1,1,0,0},{0,0,0,0},{0,0,0,0} },
            { {1,0,0,0},{1,1,0,0},{0,1,0,0},{0,0,0,0} },
            // Z (type 6) - 2 rotations
            { {1,1,0,0},{0,1,1,0},{0,0,0,0},{0,0,0,0} },
            { {0,0,1,0},{0,1,1,0},{0,1,0,0},{0,0,0,0} }
        };

        private static readonly int[] RotationCounts = { 2, 1, 4, 4, 4, 2, 2 };
        private static readonly int[] RotationStart = { 0, 2, 4, 8, 12, 16, 18 };

        private readonly int _startLevel;
        private readonly int _timePenalty;
        private readonly int _winLines;
        private readonly float _speedMultiplier;

        private int[,] _board = new int[BoardW, BoardH];
        private int _currentType;
        private int _currentRotation;
        private int _currentX;
        private int _currentY;
        private int _nextType;
        private int _linesCleared;
        private int _level;
        private bool _gameOver;
        private bool _playerWon;
        private int _endTimer;
        private int _serveDelay = 90;
        private float _gravityTimer;
        private float _gravityInterval;
        private float _keyRepeatTimer;
        private int _keyRepeatDir;
        private bool _softDropping;

        private int _clearAnimTimer;
        private bool _clearingLines;
        private bool[] _clearRows = new bool[BoardH];

        private static readonly Random Rng = new();

        public TetrisMinigame(int startLevel = 1, int timePenalty = 300, float speedMultiplier = 1.92f, int winLines = 6)
        {
            _startLevel = startLevel;
            _timePenalty = timePenalty;
            _winLines = winLines;
            _speedMultiplier = speedMultiplier;
            _level = startLevel;
            _linesCleared = 0;
            _gravityInterval = CalcGravity(_level) / _speedMultiplier;
            _nextType = Rng.Next(7);
            SpawnPiece();
        }

        private static float CalcGravity(int level)
        {
            return (float)(0.75 * Math.Pow(0.9, level - 1));
        }

        private void SpawnPiece()
        {
            _currentType = _nextType;
            _nextType = Rng.Next(7);
            _currentRotation = 0;
            _currentX = 3;
            _currentY = 0;
            _gravityTimer = 0f;
            _softDropping = false;

            if (!FitsAt(_currentType, _currentRotation, _currentX, _currentY))
            {
                _gameOver = true;
                _playerWon = false;
                AdvanceTime(_timePenalty);
                try { Game1.playSound(ModEntry.LossSoundCueId); } catch { Game1.playSound("cancel"); }
            }
        }

        private int GetRotationIndex(int type, int rotation)
        {
            return RotationStart[type] + (rotation % RotationCounts[type]);
        }

        private bool CellAt(int type, int rotIdx, int cx, int cy)
        {
            return Rotations[rotIdx, cy, cx] != 0;
        }

        private bool FitsAt(int type, int rotation, int px, int py)
        {
            int idx = GetRotationIndex(type, rotation);
            for (int cx = 0; cx < 4; cx++)
            {
                for (int cy = 0; cy < 4; cy++)
                {
                    if (!CellAt(type, idx, cx, cy)) continue;
                    int bx = px + cx;
                    int by = py + cy;
                    if (bx < 0 || bx >= BoardW || by >= BoardH) return false;
                    if (by < 0) continue;
                    if (_board[bx, by] != 0) return false;
                }
            }
            return true;
        }

        private void LockPiece()
        {
            int idx = GetRotationIndex(_currentType, _currentRotation);
            for (int cx = 0; cx < 4; cx++)
            {
                for (int cy = 0; cy < 4; cy++)
                {
                    if (!CellAt(_currentType, idx, cx, cy)) continue;
                    int bx = _currentX + cx;
                    int by = _currentY + cy;
                    if (by < 0) continue;
                    if (bx >= 0 && bx < BoardW && by < BoardH)
                        _board[bx, by] = _currentType + 1;
                }
            }
            CheckLines();
        }

        private void CheckLines()
        {
            _clearingLines = false;
            for (int y = 0; y < BoardH; y++)
            {
                bool full = true;
                for (int x = 0; x < BoardW; x++)
                {
                    if (_board[x, y] == 0) { full = false; break; }
                }
                _clearRows[y] = full;
                if (full) _clearingLines = true;
            }
            if (_clearingLines)
                _clearAnimTimer = 20;
        }

        private void ClearLines()
        {
            var toRemove = new System.Collections.Generic.List<int>();
            for (int y = 0; y < BoardH; y++)
            {
                if (_clearRows[y])
                    toRemove.Add(y);
            }
            int cleared = toRemove.Count;
            if (cleared == 0) return;

            // Remove rows top-to-bottom so indices stay valid
            // Each removed row: shift everything above it down by 1
            for (int i = 0; i < toRemove.Count; i++)
            {
                int row = toRemove[i] - i; // adjusted for already-removed rows above
                for (int y = row; y > 0; y--)
                {
                    for (int x = 0; x < BoardW; x++)
                        _board[x, y] = _board[x, y - 1];
                }
                for (int x = 0; x < BoardW; x++)
                    _board[x, 0] = 0;
            }

            for (int y = 0; y < BoardH; y++)
                _clearRows[y] = false;

            _linesCleared += cleared;
            int newLevel = _startLevel + _linesCleared / 10;
            if (newLevel != _level)
            {
                _level = newLevel;
                _gravityInterval = CalcGravity(_level) / _speedMultiplier;
            }
            Game1.playSound("coin");

            if (_linesCleared >= _winLines)
            {
                _gameOver = true;
                _playerWon = true;
                Game1.playSound("yoba");
            }
        }

        private int GhostY()
        {
            int gy = _currentY;
            while (FitsAt(_currentType, _currentRotation, _currentX, gy + 1))
                gy++;
            return gy;
        }

        private void MoveLeft()
        {
            if (FitsAt(_currentType, _currentRotation, _currentX - 1, _currentY))
            {
                _currentX--;
                Game1.playSound("smallSelect");
            }
        }

        private void MoveRight()
        {
            if (FitsAt(_currentType, _currentRotation, _currentX + 1, _currentY))
            {
                _currentX++;
                Game1.playSound("smallSelect");
            }
        }

        private void Rotate()
        {
            int newRot = (_currentRotation + 1) % RotationCounts[_currentType];
            if (FitsAt(_currentType, newRot, _currentX, _currentY))
            {
                _currentRotation = newRot;
                Game1.playSound("smallSelect");
                return;
            }
            if (FitsAt(_currentType, newRot, _currentX - 1, _currentY))
            {
                _currentX--;
                _currentRotation = newRot;
                Game1.playSound("smallSelect");
                return;
            }
            if (FitsAt(_currentType, newRot, _currentX + 1, _currentY))
            {
                _currentX++;
                _currentRotation = newRot;
                Game1.playSound("smallSelect");
                return;
            }
            if (FitsAt(_currentType, newRot, _currentX, _currentY - 1))
            {
                _currentY--;
                _currentRotation = newRot;
                Game1.playSound("smallSelect");
            }
        }

        private void HardDrop()
        {
            int gy = GhostY();
            _currentY = gy;
            LockPiece();
            Game1.playSound("smallSelect");
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
            if (_clearingLines)
            {
                _clearAnimTimer--;
                if (_clearAnimTimer <= 0)
                {
                    ClearLines();
                    _clearingLines = false;
                    SpawnPiece();
                }
                return false;
            }

            float dt = (float)time.ElapsedGameTime.TotalSeconds;

            float interval = _softDropping ? Math.Min(_gravityInterval, 0.05f) : _gravityInterval;
            _gravityTimer += dt;
            if (_gravityTimer >= interval)
            {
                _gravityTimer = 0f;
                if (FitsAt(_currentType, _currentRotation, _currentX, _currentY + 1))
                {
                    _currentY++;
                }
                else
                {
                    LockPiece();
                    if (!_clearingLines)
                        SpawnPiece();
                }
            }

            if (_keyRepeatDir != 0)
            {
                _keyRepeatTimer += dt;
                if (_keyRepeatTimer >= 0.12f)
                {
                    _keyRepeatTimer = 0f;
                    if (_keyRepeatDir < 0) MoveLeft();
                    else MoveRight();
                }
            }

            return false;
        }

        public void draw(SpriteBatch b)
        {
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            try
            {
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, ScreenW, ScreenH), Color.Black * 0.85f);

                DrawBoard(b);
                DrawCurrentPiece(b);
                DrawGhost(b);
                DrawNextPiece(b);
                DrawHUD(b);

                var bigFont = Game1.dialogueFont ?? Game1.smallFont;

                if (_serveDelay > 0)
                    DrawCenteredText(b, bigFont, "GET READY...", ScreenW / 2f, ScreenH / 2f, Color.Yellow);
                else if (_gameOver)
                {
                    DrawCenteredText(b, bigFont, _playerWon ? "YOU WIN!" : $"GAME OVER  -  +{_timePenalty / 100}h", ScreenW / 2f, ScreenH / 2f - 20, _playerWon ? Color.LimeGreen : Color.Red);
                    DrawCenteredText(b, bigFont, $"Lines: {_linesCleared}/{_winLines}   Level: {_level}", ScreenW / 2f, ScreenH / 2f + 20, Color.White);
                }
            }
            finally
            {
                b.End();
            }
        }

        private void DrawBoard(SpriteBatch b)
        {
            int bw = BoardW * CellSize;
            int bh = BoardH * CellSize;

            b.Draw(Game1.fadeToBlackRect, new Rectangle(BoardX - 2, BoardY - 2, bw + 4, bh + 4), Color.White * 0.3f);

            for (int x = 0; x < BoardW; x++)
            {
                for (int y = 0; y < BoardH; y++)
                {
                    Rectangle cellRect = new Rectangle(BoardX + x * CellSize, BoardY + y * CellSize, CellSize, CellSize);
                    if (_board[x, y] != 0)
                    {
                        Color c = PieceColors[_board[x, y] - 1];
                        if (_clearingLines && _clearRows[y])
                            c = Color.White;
                        b.Draw(Game1.staminaRect, cellRect, c);
                        b.Draw(Game1.staminaRect, new Rectangle(cellRect.X, cellRect.Y, CellSize, 2), Color.White * 0.4f);
                        b.Draw(Game1.staminaRect, new Rectangle(cellRect.X, cellRect.Y + CellSize - 2, CellSize, 2), Color.White * 0.4f);
                        b.Draw(Game1.staminaRect, new Rectangle(cellRect.X, cellRect.Y, 2, CellSize), Color.White * 0.4f);
                        b.Draw(Game1.staminaRect, new Rectangle(cellRect.X + CellSize - 2, cellRect.Y, 2, CellSize), Color.White * 0.4f);
                    }
                    else
                    {
                        b.Draw(Game1.staminaRect, cellRect, Color.DarkSlateGray * 0.15f);
                    }
                }
            }
        }

        private void DrawCurrentPiece(SpriteBatch b)
        {
            if (_gameOver || _serveDelay > 0 || _clearingLines) return;
            int idx = GetRotationIndex(_currentType, _currentRotation);
            Color c = PieceColors[_currentType];
            for (int cx = 0; cx < 4; cx++)
            {
                for (int cy = 0; cy < 4; cy++)
                {
                    if (!CellAt(_currentType, idx, cx, cy)) continue;
                    int bx = _currentX + cx;
                    int by = _currentY + cy;
                    if (by < 0) continue;
                    Rectangle rect = new Rectangle(BoardX + bx * CellSize, BoardY + by * CellSize, CellSize, CellSize);
                    b.Draw(Game1.staminaRect, rect, c);
                    b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, CellSize, 2), Color.White * 0.4f);
                    b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y + CellSize - 2, CellSize, 2), Color.White * 0.4f);
                    b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, 2, CellSize), Color.White * 0.4f);
                    b.Draw(Game1.staminaRect, new Rectangle(rect.X + CellSize - 2, rect.Y, 2, CellSize), Color.White * 0.4f);
                }
            }
        }

        private void DrawGhost(SpriteBatch b)
        {
            if (_gameOver || _serveDelay > 0 || _clearingLines) return;
            int gy = GhostY();
            if (gy == _currentY) return;
            int idx = GetRotationIndex(_currentType, _currentRotation);
            Color c = PieceColors[_currentType] * 0.3f;
            for (int cx = 0; cx < 4; cx++)
            {
                for (int cy = 0; cy < 4; cy++)
                {
                    if (!CellAt(_currentType, idx, cx, cy)) continue;
                    int bx = _currentX + cx;
                    int by = gy + cy;
                    if (by < 0) continue;
                    Rectangle rect = new Rectangle(BoardX + bx * CellSize, BoardY + by * CellSize, CellSize, CellSize);
                    b.Draw(Game1.staminaRect, rect, c);
                    b.Draw(Game1.staminaRect, new Rectangle(rect.X + 2, rect.Y + 2, CellSize - 4, CellSize - 4), Color.White * 0.15f);
                }
            }
        }

        private void DrawNextPiece(SpriteBatch b)
        {
            int previewX = BoardX + BoardW * CellSize + 30;
            int previewY = BoardY + 20;

            var font = Game1.smallFont;
            b.DrawString(font, "NEXT", new Vector2(previewX, previewY), Color.White);

            int idx = GetRotationIndex(_nextType, 0);
            Color c = PieceColors[_nextType];
            for (int cx = 0; cx < 4; cx++)
            {
                for (int cy = 0; cy < 4; cy++)
                {
                    if (!CellAt(_nextType, idx, cx, cy)) continue;
                    Rectangle rect = new Rectangle(previewX + cx * CellSize, previewY + 30 + cy * CellSize, CellSize, CellSize);
                    b.Draw(Game1.staminaRect, rect, c);
                    b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, CellSize, 2), Color.White * 0.4f);
                    b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y + CellSize - 2, CellSize, 2), Color.White * 0.4f);
                    b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, 2, CellSize), Color.White * 0.4f);
                    b.Draw(Game1.staminaRect, new Rectangle(rect.X + CellSize - 2, rect.Y, 2, CellSize), Color.White * 0.4f);
                }
            }
        }

        private void DrawHUD(SpriteBatch b)
        {
            var font = Game1.dialogueFont ?? Game1.smallFont;
            int infoX = BoardX + BoardW * CellSize + 30;
            int infoY = BoardY + 200;

            DrawBoldString(b, font, "TETRIS", new Vector2(BoardX, BoardY - 45), Color.White);

            var smallFont = Game1.smallFont;
            b.DrawString(smallFont, $"Lines: {_linesCleared}/{_winLines}", new Vector2(infoX, infoY), Color.White);
            b.DrawString(smallFont, $"Level: {_level}", new Vector2(infoX, infoY + 25), Color.White);
            b.DrawString(smallFont, "Arrows: Move/Drop", new Vector2(infoX, infoY + 65), Color.Gray);
            b.DrawString(smallFont, "Space: Rotate", new Vector2(infoX, infoY + 85), Color.Gray);
            b.DrawString(smallFont, "Esc: Forfeit", new Vector2(infoX, infoY + 105), Color.Gray);
        }

        private static void DrawCenteredText(SpriteBatch b, SpriteFont font, string text, float x, float y, Color color)
        {
            Vector2 size = font.MeasureString(text);
            b.DrawString(font, text, new Vector2(x - size.X / 2f, y - size.Y / 2f), color);
        }

        private static void DrawBoldString(SpriteBatch b, SpriteFont font, string text, Vector2 pos, Color color)
        {
            b.DrawString(font, text, pos + new Vector2(2, 2), Color.Black * 0.5f);
            b.DrawString(font, text, pos + new Vector2(1, 0), color);
            b.DrawString(font, text, pos + new Vector2(-1, 0), color);
            b.DrawString(font, text, pos, color);
        }

        private static void AdvanceTime(int minutes)
        {
            int newTime = Game1.timeOfDay + minutes;
            while (newTime >= 2400) newTime -= 2400;
            Game1.timeOfDay = newTime;
        }

        public void receiveKeyPress(Keys key)
        {
            if (_serveDelay > 0) return;
            if (_gameOver) return;
            if (_clearingLines) return;

            if (key == Keys.Escape)
            {
                _gameOver = true;
                _playerWon = false;
                AdvanceTime(_timePenalty);
                try { Game1.playSound(ModEntry.LossSoundCueId); } catch { Game1.playSound("cancel"); }
                return;
            }

            if (key == Keys.Left)
            {
                MoveLeft();
                _keyRepeatDir = -1;
                _keyRepeatTimer = -0.15f;
            }
            else if (key == Keys.Right)
            {
                MoveRight();
                _keyRepeatDir = 1;
                _keyRepeatTimer = -0.15f;
            }
            else if (key == Keys.Up || key == Keys.Space)
            {
                Rotate();
            }
            else if (key == Keys.Down)
            {
                _softDropping = true;
            }
        }

        public void receiveKeyRelease(Keys key)
        {
            if (key == Keys.Left && _keyRepeatDir < 0) _keyRepeatDir = 0;
            if (key == Keys.Right && _keyRepeatDir > 0) _keyRepeatDir = 0;
            if (key == Keys.Down) _softDropping = false;
        }

        public void receiveLeftClick(int x, int y, bool playSound = true) { }
        public void releaseLeftClick(int x, int y) { }
        public void receiveRightClick(int x, int y, bool playSound = true) { }
        public void releaseRightClick(int x, int y) { }
        public void leftClickHeld(int x, int y) { }
        public void rightClickHeld(int x, int y) { }
        public bool forceQuit() { return false; }
        public void changeScreenSize() { }
        public void unload() { }
        public void receiveEventPoke(int data) { }
        public string minigameId()
        {
            return _startLevel >= 10 ? "HardTetris" : "Tetris";
        }

        public bool doMainGameUpdates() { return false; }
        public bool overrideFreeMouseMovement() { return true; }
    }
}