using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;

namespace StardewChaos
{
    public static class AnimorphManager
    {
        private static bool _active;
        private static int _ticksRemaining;
        private static int _ticksTotal;
        private static float _originalSpeed;
        private static string _animalName;
        private static Texture2D _animalTexture;
        private static List<AnimalDef> _validAnimals;
        private static bool _scanned;
        private static int _directionRow;
        private static int _animFrame;
        private static int _animTimer;
        private static int _spriteWidth;
        private static int _spriteHeight;
        private static int _currentAnimCols;
        private static int _currentDirRows;
        private const int AnimInterval = 8;

        private enum AnimalKind { Farm, Pet, Horse }

        private class AnimalDef
        {
            public string Name;
            public string TexturePath;
            public float Speed;
            public AnimalKind Kind;
            public string SubType;
            public int AnimCols;
            public int DirRows;
        }

        private static readonly AnimalDef[] CandidateAnimals =
        {
            new AnimalDef { Name = "Dog", TexturePath = "Animals\\dog", Speed = 3.5f, Kind = AnimalKind.Pet, SubType = "Dog", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Cat", TexturePath = "Animals\\cat", Speed = 3.5f, Kind = AnimalKind.Pet, SubType = "Cat", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Horse", TexturePath = "Animals\\horse", Speed = 4.5f, Kind = AnimalKind.Horse, SubType = "", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Turtle", TexturePath = "Animals\\turtle", Speed = 1.5f, Kind = AnimalKind.Pet, SubType = "Turtle", AnimCols = 1, DirRows = 1 },
            new AnimalDef { Name = "Chicken", TexturePath = "Animals\\White Chicken", Speed = 2f, Kind = AnimalKind.Farm, SubType = "White Chicken", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Duck", TexturePath = "Animals\\Duck", Speed = 2.5f, Kind = AnimalKind.Farm, SubType = "Duck", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Rabbit", TexturePath = "Animals\\Rabbit", Speed = 3f, Kind = AnimalKind.Farm, SubType = "Rabbit", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Cow", TexturePath = "Animals\\White Cow", Speed = 2.5f, Kind = AnimalKind.Farm, SubType = "White Cow", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Goat", TexturePath = "Animals\\Goat", Speed = 2.5f, Kind = AnimalKind.Farm, SubType = "Goat", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Sheep", TexturePath = "Animals\\Sheep", Speed = 2.5f, Kind = AnimalKind.Farm, SubType = "Sheep", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Pig", TexturePath = "Animals\\Pig", Speed = 2f, Kind = AnimalKind.Farm, SubType = "Pig", AnimCols = 4, DirRows = 4 },
            new AnimalDef { Name = "Ostrich", TexturePath = "Animals\\Ostrich", Speed = 3.5f, Kind = AnimalKind.Farm, SubType = "Ostrich", AnimCols = 4, DirRows = 4 },
        };

        private static void ResolveSpriteSize(AnimalDef animal)
        {
            try
            {
                AnimatedSprite sprite = null;
                switch (animal.Kind)
                {
                    case AnimalKind.Farm:
                        var fa = new FarmAnimal(animal.SubType, Game1.random.Next(), Game1.player?.UniqueMultiplayerID ?? 0L);
                        sprite = fa.Sprite;
                        break;
                    case AnimalKind.Pet:
                        var pet = new Pet();
                        sprite = pet.Sprite;
                        break;
                    case AnimalKind.Horse:
                        var horse = new Horse();
                        sprite = horse.Sprite;
                        break;
                }

                if (sprite != null)
                {
                    _spriteWidth = sprite.SpriteWidth;
                    _spriteHeight = sprite.SpriteHeight;
                    ModEntry.Instance?.Monitor?.Log($"Animorph: resolved sprite size from {animal.Kind} ({animal.SubType}): {_spriteWidth}x{_spriteHeight}", LogLevel.Trace);
                    return;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Monitor?.Log($"Animorph: failed to resolve sprite size via entity for {animal.Name}: {ex.Message}", LogLevel.Warn);
            }

            _spriteWidth = _animalTexture.Width / Math.Max(1, _currentAnimCols);
            _spriteHeight = _animalTexture.Height / Math.Max(1, _currentDirRows);
        }

        private static void ScanValidAnimals()
        {
            _validAnimals = new List<AnimalDef>();
            foreach (var animal in CandidateAnimals)
            {
                try
                {
                    var tex = Game1.content.Load<Texture2D>(animal.TexturePath);
                    if (tex != null)
                    {
                        _validAnimals.Add(animal);
                        ModEntry.Instance?.Monitor?.Log($"Animorph: verified texture {animal.TexturePath} ({animal.Name})", LogLevel.Trace);
                    }
                }
                catch { }
            }
            _scanned = true;
            ModEntry.Instance?.Monitor?.Log($"Animorph: {_validAnimals.Count} valid animal textures found.", LogLevel.Info);
        }

        public static bool IsActive => _active;

        public static void Start()
        {
            try
            {
                if (!_scanned) ScanValidAnimals();
                if (_validAnimals == null || _validAnimals.Count == 0)
                {
                    ModEntry.Instance?.Monitor?.Log("Animorph: no valid animal textures available, aborting.", LogLevel.Warn);
                    return;
                }

                var animal = _validAnimals[Game1.random.Next(_validAnimals.Count)];
                _animalName = animal.Name;

                _animalTexture = Game1.content.Load<Texture2D>(animal.TexturePath);
                if (_animalTexture == null)
                {
                    ModEntry.Instance?.Monitor?.Log($"Animorph: could not load {animal.TexturePath} at runtime, aborting.", LogLevel.Warn);
                    return;
                }

                _originalSpeed = Game1.player.addedSpeed;
                Game1.player.addedSpeed = (int)Math.Round(animal.Speed - 5f);

                ResolveSpriteSize(animal);

                _currentAnimCols = _spriteWidth > 0 ? _animalTexture.Width / _spriteWidth : animal.AnimCols;
                _currentDirRows = _spriteHeight > 0 ? _animalTexture.Height / _spriteHeight : animal.DirRows;
                if (_currentAnimCols < 1) _currentAnimCols = 1;
                if (_currentDirRows < 1) _currentDirRows = 1;

                _ticksTotal = 45 * 60;
                _ticksRemaining = _ticksTotal;
                _directionRow = 0;
                _animFrame = 0;
                _animTimer = 0;
                _active = true;

                ModEntry.Instance?.Monitor?.Log($"Animorph: player became {animal.Name} ({animal.TexturePath}) for 45s. Texture: {_animalTexture.Width}x{_animalTexture.Height}, Sprite: {_spriteWidth}x{_spriteHeight}, AnimCols: {_currentAnimCols}, DirRows: {_currentDirRows}", LogLevel.Info);
                Game1.playSound("dirtyHit");
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Monitor?.Log($"Animorph start failed: {ex.Message}", LogLevel.Error);
            }
        }

        public static void Stop()
        {
            if (!_active) return;
            try
            {
                Game1.player.addedSpeed = (int)_originalSpeed;
                _active = false;
                _animalTexture = null;
                ModEntry.Instance?.Monitor?.Log("Animorph: reverted to normal.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Monitor?.Log($"Animorph stop failed: {ex.Message}", LogLevel.Error);
            }
        }

        public static void OnRendered(object sender, RenderedEventArgs e)
        {
            if (!_active || _animalTexture == null) return;
            if (Game1.player == null || Game1.currentLocation == null) return;

            try
            {
                var b = e.SpriteBatch;

                float zoom = Game1.options.desiredBaseZoomLevel;
                float px = (Game1.player.Position.X - Game1.viewport.X) * zoom;
                float py = (Game1.player.Position.Y - Game1.viewport.Y) * zoom;

                int frameW = _spriteWidth;
                int frameH = _spriteHeight;
                if (frameW < 1) frameW = _animalTexture.Width / Math.Max(1, _currentAnimCols);
                if (frameH < 1) frameH = _animalTexture.Height / Math.Max(1, _currentDirRows);

                int maxCol = _currentAnimCols - 1;
                int maxRow = _currentDirRows - 1;
                if (maxCol < 0) maxCol = 0;
                if (maxRow < 0) maxRow = 0;

                int col = _animFrame;
                int row = _directionRow;
                if (col > maxCol) col = 0;
                if (row > maxRow) row = 0;

                var srcRect = new Rectangle(col * frameW, row * frameH, frameW, frameH);
                int dw = (int)(frameW * zoom * 2);
                int dh = (int)(frameH * zoom * 2);
                int dx = (int)(px - dw / 2f);
                int dy = (int)(py - dh + 8 * zoom);

                b.Draw(_animalTexture, new Rectangle(dx, dy, dw, dh), srcRect, Color.White);
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Monitor?.Log($"Animorph OnRendered error: {ex.Message}", LogLevel.Error);
            }
        }

        public static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_active) return;
            if (!StardewModdingAPI.Context.IsWorldReady) return;
            if (ModEntry.Instance.ShouldPauseTimers()) return;

            if (_currentDirRows >= 4)
            {
                _directionRow = (6 - Game1.player.FacingDirection) % 4;
            }
            else if (_currentDirRows >= 2)
            {
                _directionRow = (Game1.player.FacingDirection == 1 || Game1.player.FacingDirection == 3) ? 1 : 0;
            }

            if (Game1.player.isMoving() && _currentAnimCols > 1)
            {
                _animTimer++;
                if (_animTimer >= AnimInterval)
                {
                    _animTimer = 0;
                    int maxFrame = _currentAnimCols - 1;
                    if (maxFrame < 1) maxFrame = 1;
                    _animFrame = (_animFrame % maxFrame) + 1;
                }
            }
            else if (!Game1.player.isMoving())
            {
                _animFrame = 0;
                _animTimer = 0;
            }

            _ticksRemaining--;
            if (_ticksRemaining <= 0)
            {
                Stop();
                return;
            }
        }

        public static float Progress => _active && _ticksTotal > 0 ? (float)_ticksRemaining / _ticksTotal : 0f;
        public static string CurrentAnimal => _animalName ?? "";
    }
}