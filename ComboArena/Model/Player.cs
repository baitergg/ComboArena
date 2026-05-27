using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
namespace ComboArena.Model
{
    public class Player : Entity
    {
        public float AttackDamage = 10f;
        public float AttackRange = 70f;
        public float AttackCooldown = 0.5f;
        public const float AttackArcAngle = (float)(60f * (Math.PI / 180));
        private float _attackTimer;
        private float _attackVisualTimer;
        private const float AttackVisualDuration = 0.3f; // 300 мс отображения атаки
        public Vector2 FacingDirection { get; private set; }
        public bool IsAttacking { get; private set; }
        public int Level { get; private set; } = 1;
        public float Experience { get; private set; }
        public float ExperienceToNextLevel { get; private set; } = 100f;
        public event Action<int> OnLevelUp;
        public float VampirismPercent;
        public float ExperienceMultiplier = 1f;
        public List<Perk> ActivePerks { get; } = [];
        public int ComboCount { get; private set; }
        public int MaxCombo { get; private set; }
        public float ComboDamageMultiplier => 1f + (ComboCount * 0.05f);
        public float ComboExperienceMultiplier => 1f + (ComboCount * 0.03f);
        public float ComboTimer { get; private set; }
        public const float ComboTimeout = 3f;
        private Vector2 _movementDirection = Vector2.Zero;
        private bool _wantToAttack;

        public Player(float x, float y)
            : base(x, y, width: 120, height: 100, maxHealth: 100, speed: 200)
        {
            FacingDirection = Vector2.UnitX;
            CalculateNextLevelExp();
            CollisionScale = 0.3f;
        }
        public void SetMovementDirection(Vector2 direction)
        {
            _movementDirection = direction;
            if (_movementDirection.LengthSquared() > 0)
            {
                _movementDirection.Normalize();
            }
        }
        public void SetAttacking(bool attacking)
        {
            _wantToAttack = attacking;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_movementDirection.X != 0)
            {
                FacingDirection = _movementDirection.X > 0 ? Vector2.UnitX : -Vector2.UnitX;
            }
            Velocity = _movementDirection * Speed;
            if (_wantToAttack && _attackTimer <= 0)
            {
                IsAttacking = true;
                _attackTimer = AttackCooldown;
                _attackVisualTimer = AttackVisualDuration;
            }
            
            // Обновление визуального таймера атаки
            if (_attackVisualTimer > 0)
            {
                _attackVisualTimer -= delta;
                IsAttacking = true;
            }
            else
            {
                IsAttacking = false;
            }
            
            if (_attackTimer > 0)
                _attackTimer -= delta;
            if (ComboCount > 0)
            {
                ComboTimer -= delta;
                if (ComboTimer <= 0)
                {
                    ResetCombo();
                }
            }
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
            // прямоугольник, в который вписывается дуга атаки
            var playerCenter = Position + new Vector2(Width / 2, Height / 2);
            const float halfArc = AttackArcAngle / 2;
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
        private void CalculateNextLevelExp()
        {
            ExperienceToNextLevel = 100f * (float)Math.Pow(Level, 1.5);
        }
        public void AddExperience(float amount)
        {
            var finalAmount = amount * ExperienceMultiplier;
            Experience += finalAmount;
            while (Experience >= ExperienceToNextLevel)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                CalculateNextLevelExp();
                OnLevelUp?.Invoke(Level);
            }
        }
        public void HealOnKill()
        {
            var healAmount = MaxHealth * VampirismPercent;
            Health = Math.Min(Health + healAmount, MaxHealth);
        }
        public void ApplyPerk(Perk perk)
        {
            perk.Apply(this);
            ActivePerks.Add(perk);
        }
        public void AddCombo()
        {
            ComboCount++;
            ComboTimer = ComboTimeout;
            if (ComboCount > MaxCombo)
            {
                MaxCombo = ComboCount;
            }
        }
        public void ResetCombo()
        {
            ComboCount = 0;
            ComboTimer = 0f;
        }
        public void ResetAttackCooldown()
        {
            _attackTimer = AttackCooldown;
        }
        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            if (damage > 0)
            {
                ResetCombo();
            }
        }
        public float GetComboBonusDamage(float baseDamage)
        {
            return baseDamage * ComboDamageMultiplier;
        }
        public float GetComboBonusExperience(float baseExperience)
        {
            return baseExperience * ComboExperienceMultiplier;
        }
        public void ModifyAttackDamage(float multiplier)
        {
            AttackDamage *= multiplier;
        }
        public void ModifyAttackCooldown(float multiplier)
        {
            AttackCooldown *= multiplier;
        }
        public void ModifyAttackRange(float multiplier)
        {
            AttackRange *= multiplier;
        }
        public void ModifySpeed(float multiplier)
        {
            Speed *= multiplier;
        }
        public void ModifyMaxHealth(float multiplier)
        {
            var oldMaxHealth = MaxHealth;
            MaxHealth *= multiplier;
            Health += MaxHealth - oldMaxHealth;
        }
        public void AddVampirismPercent(float value)
        {
            VampirismPercent += value;
        }
        public void AddExperienceMultiplier(float value)
        {
            ExperienceMultiplier += value;
        }
    }
}
