using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewChaos
{
    public class GrieferJesusNPC : ShadowBrute
    {
        private float _chaseSpeed;
        private int _contactDamage;
        private bool _invincible;
        private int _maxHp;
        private int _tickCounter;
        private Texture2D _villagerTexture;
        private Rectangle _villagerSourceRect;
        private bool _hasVillagerSprite;

        public GrieferJesusNPC(Vector2 position, float chaseSpeed = 3.5f, int contactDamage = 35, int health = 999999, bool invincible = true)
            : base(position)
        {
            _chaseSpeed = chaseSpeed;
            _contactDamage = contactDamage;
            _invincible = invincible;
            _maxHp = invincible ? 999999 : health;
            Health = _maxHp;
            MaxHealth = _maxHp;
            DamageToFarmer = contactDamage;
            Name = "GrieferJesus";
            displayName = "Griefer Jesus";
            SimpleNonVillagerNPC = true;
            _hasVillagerSprite = false;

            LoadVillagerSprite();
        }

        private void LoadVillagerSprite()
        {
            try
            {
                List<NPC> villagers = new List<NPC>();
                foreach (GameLocation loc in Game1.locations)
                {
                    if (loc?.characters == null) continue;
                    foreach (var npc in loc.characters)
                    {
                        if (npc != null && npc.IsVillager)
                            villagers.Add(npc);
                    }
                }

                if (villagers.Count > 0)
                {
                    var pick = villagers[Game1.random.Next(villagers.Count)];
                    if (pick.Sprite?.Texture != null)
                    {
                        _villagerTexture = pick.Sprite.Texture;
                        _villagerSourceRect = pick.Sprite.SourceRect;
                        _hasVillagerSprite = true;
                    }
                }
            }
            catch { }
        }

        public override void draw(SpriteBatch b)
        {
            if (_hasVillagerSprite && _villagerTexture != null && !(_villagerTexture.IsDisposed))
            {
                try
                {
                    Vector2 pos = Game1.GlobalToLocal(Position);
                    pos.Y -= 32f;
                    b.Draw(_villagerTexture, pos, _villagerSourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.5f);
                }
                catch
                {
                    base.draw(b);
                }
            }
            else
            {
                base.draw(b);
            }
        }

        public override void behaviorAtGameTick(GameTime time)
        {
            if (_invincible && Health < _maxHp)
                Health = _maxHp;

            if (Game1.player == null || currentLocation == null) return;

            _tickCounter++;

            if (withinPlayerThreshold() || _tickCounter % 3 == 0)
            {
                Farmer p = Game1.player;

                float dx = p.Position.X - Position.X;
                float dy = p.Position.Y - Position.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                if (dist > 0.1f)
                {
                    float vx = (dx / dist) * _chaseSpeed;
                    float vy = (dy / dist) * _chaseSpeed;
                    Position = new Vector2(Position.X + vx, Position.Y + vy);
                }

                if (dist < 40f)
                {
                    if (p.CurrentTool is not MeleeWeapon)
                    {
                        p.takeDamage(_contactDamage, true, this);
                        Game1.playSound("hitEnemy");
                    }
                }
            }

            if (Health <= 0 && !_invincible)
            {
                try { Game1.playSound("shadowbeastHit"); } catch { }
                try { Game1.createRadialDebris(currentLocation, 12, (int)Tile.X, (int)Tile.Y, 8, false); } catch { }
            }
        }
    }
}