using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewChaos
{
    public class HostileVillager : ShadowBrute
    {
        private float _chaseSpeed;
        private int _contactDamage;
        private bool _invincible;
        private int _tickCounter;
        private int _maxHp;

        public HostileVillager(Vector2 position, float chaseSpeed = 3f, int contactDamage = 20, int health = 200, bool invincible = false)
            : base(position)
        {
            _chaseSpeed = chaseSpeed;
            _contactDamage = contactDamage;
            _invincible = invincible;
            _maxHp = invincible ? 999999 : health;

            Health = _maxHp;
            MaxHealth = _maxHp;
            DamageToFarmer = contactDamage;
        }

        protected virtual void OnKilled()
        {
            try { Game1.playSound("shadowbeastHit"); } catch { }
            try { Game1.createRadialDebris(currentLocation, 12, (int)Tile.X, (int)Tile.Y, 8, false); } catch { }
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
                    if (p.TemporaryItem is not MeleeWeapon && p.CurrentTool is not MeleeWeapon)
                    {
                        p.takeDamage(_contactDamage, true, this);
                        Game1.playSound("hitEnemy");
                    }
                }
            }

            if (Health <= 0 && !_invincible)
            {
                OnKilled();
            }
        }
    }
}