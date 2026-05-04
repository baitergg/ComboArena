using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ComboArena.Model
{
    public class Player : Entity
    {
        public float AttackDamage { get; set; } = 20f;
        public float AttackRange { get; set; } = 50f;
        public float AttackCooldown { get; set; } = 0.5f;
        public float AttackArcAngle { get; set; } = (float)(60f * (Math.PI/180));
        private float _attackTimer = 0f;
        public bool IsAttacking { get; private set; }
        public Vector2 FacingDirection { get; private set; }

        public Player(float x, float y)
            : base(x, y, width: 30, height: 30, maxHealth: 100, speed: 200)
        {
            FacingDirection = Vector2.UnitX;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            var keyboardState = Keyboard.GetState();
            var direction = Vector2.Zero;
            
            if (keyboardState.IsKeyDown(Keys.W))
                direction.Y = -1;
            if (keyboardState.IsKeyDown(Keys.S))
                direction.Y = 1;
            if (keyboardState.IsKeyDown(Keys.A))
                direction.X = -1;
            if (keyboardState.IsKeyDown(Keys.D))
                direction.X = 1;

            FacingDirection = direction.X switch
            {
                > 0 => Vector2.UnitX,
                < 0 => -Vector2.UnitX,
                _ => FacingDirection
            };
            
            Velocity = direction * Speed;
            
            if (keyboardState.IsKeyDown(Keys.Space) && _attackTimer <= 0)
            {
                IsAttacking = true;
                _attackTimer = AttackCooldown;
            }
            else
            {
                IsAttacking = false;
            }
            
            if (_attackTimer > 0)
                _attackTimer -= delta;
        }

        public bool IsInAttackArc(Entity target)
        {
            var playerCenter = Position + new Vector2(Width / 2, Height / 2);
            var targetCenter = target.Position + new Vector2(target.Width / 2, target.Height / 2);
            var toTarget = targetCenter - playerCenter;
            
            var distance = toTarget.Length();
            if (distance > AttackRange)
                return false;
            
            if (distance == 0) return false;
            toTarget.Normalize();
            
            var dot = Vector2.Dot(FacingDirection, toTarget);
            var angle = (float)Math.Acos(dot);
            
            return Math.Abs(angle) <= AttackArcAngle / 2;
        }

        public Rectangle GetAttackBounds()
        {
            var playerCenter = Position + new Vector2(Width / 2, Height / 2);
            var halfArc = AttackArcAngle / 2;
            var maxY = AttackRange * (float)Math.Sin(halfArc);
            
            int left, right;
            if (FacingDirection.X > 0)
            {
                left = (int)playerCenter.X;
                right = (int)(playerCenter.X + AttackRange);
            }
            else 
            {
                left = (int)(playerCenter.X - AttackRange);
                right = (int)playerCenter.X;
            }
            var top = (int)(playerCenter.Y - maxY);
            var bottom = (int)(playerCenter.Y + maxY);
            
            return new Rectangle(left, top, right - left, bottom - top);
        }
    }
}