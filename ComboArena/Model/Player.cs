using ComboArena.Core;
using ComboArena.Model.Abilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ComboArena.Model
{
    /// <summary>
    /// Игрок - главный управляемый персонаж.
    /// Может двигаться, атаковать врагов в дуге перед собой,
    /// получать уровни и опыт, использовать активные способности,
    /// накапливать комбо и применять перки.
    /// </summary>
    public class Player : Entity
    {
        public const float ComboTimeout = 3f;
        
        public float AttackDamage { get; private set; } = 10f;

        private float AttackRange { get; set; } = 70f;

        private float AttackCooldown { get; set; } = 0.5f;
        
        public Vector2 FacingDirection { get; private set; }

        public bool IsAttacking { get; private set; }
        
        public int Level { get; private set; } = 1;

        public float Experience { get; private set; }

        public float ExperienceToNextLevel { get; private set; } = 100f;

        public event Action<int> OnLevelUp;

        public float VampirismPercent { get; private set; }

        private float ExperienceMultiplier { get; set; } = 1f;

        public List<Perk> ActivePerks { get; } = [];
        
        public int ComboCount { get; private set; }

        private int MaxCombo { get; set; }

        public float ComboDamageMultiplier => 1f + (ComboCount * 0.05f);

        public float ComboExperienceMultiplier => 1f + (ComboCount * 0.03f);

        public float ComboTimer { get; private set; }
        
        public ActiveAbility Ability { get; private set; }

        public bool HasAbility => Ability != null;

        public bool IsInvulnerable { get; set; }
        
        private Vector2 _movementDirection = Vector2.Zero;

        private bool _wantToAttack;

        private float _attackTimer;

        private float _attackVisualTimer;

        private const float AttackVisualDuration = 0.3f;

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

            // Обновление направления взгляда
            if (_movementDirection.X != 0)
            {
                FacingDirection = _movementDirection.X > 0 ? Vector2.UnitX : -Vector2.UnitX;
            }

            // Применение скорости движения
            Velocity = _movementDirection * Speed;

            // Обработка начала атаки
            if (_wantToAttack && _attackTimer <= 0)
            {
                IsAttacking = true;
                _attackTimer = AttackCooldown;
                _attackVisualTimer = AttackVisualDuration;
            }

            // Обработка визуального таймера атаки
            if (_attackVisualTimer > 0)
            {
                _attackVisualTimer -= delta;
                IsAttacking = true;
            }
            else
            {
                IsAttacking = false;
            }

            // Обработка КД атаки
            if (_attackTimer > 0)
            {
                _attackTimer -= delta;
            }

            // Обработка таймера комбо
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
            return GetAttackBounds().Intersects(target.GetCollisionBounds());
        }

        public Rectangle GetAttackBounds()
        {
            var playerCenter = Position + new Vector2(Width / 2, Height / 2);

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

            var top = (int)(playerCenter.Y - Height / 2);
            var bottom = (int)(playerCenter.Y + Height / 2);
            return new Rectangle(left, top, right - left, bottom - top);
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
                EventBus.Instance.Publish(new LevelUpEvent(Level));
            }
        }

        public void HealOnKill()
        {
            var oldHealth = Health;
            var healAmount = MaxHealth * VampirismPercent;
            Health = Math.Min(Health + healAmount, MaxHealth);
            EventBus.Instance.Publish(new HealthChangedEvent(this, oldHealth, Health));
        }

        public override void TakeDamage(float damage)
        {
            if (IsInvulnerable) return;
            base.TakeDamage(damage);
            if (damage > 0)
            {
                ResetCombo();
            }
        }
        public void ApplyPerk(Perk perk)
        {
            perk.Apply(this);
            ActivePerks.Add(perk);
        }

        public void HealToFull()
        {
            var oldHealth = Health;
            Health = MaxHealth;
            EventBus.Instance.Publish(new HealthChangedEvent(this, oldHealth, Health));
        }

        public void RemoveAbility()
        {
            Ability = null;
        }


        public void AddCombo()
        {
            ComboCount++;
            ComboTimer = ComboTimeout;
            if (ComboCount > MaxCombo)
            {
                MaxCombo = ComboCount;
            }
            EventBus.Instance.Publish(new ComboEvent(ComboCount, MaxCombo, ComboDamageMultiplier, ComboExperienceMultiplier));
        }

        public void ResetCombo()
        {
            ComboCount = 0;
            ComboTimer = 0f;
            EventBus.Instance.Publish(new ComboEvent(0, MaxCombo, 1f, 1f));
        }

        public void ResetAttackCooldown()
        {
            _attackTimer = AttackCooldown;
        }


        public void SetAbility(ActiveAbility ability)
        {
            Ability = ability;
        }

        public void UseAbility(List<Enemy> enemies)
        {
            Ability?.Activate(this, enemies);
        }

        public void UpdateAbility(float delta, List<Enemy> enemies)
        {
            Ability?.Update(delta, this, enemies);
        }

        public void ModifyAttackDamage(float multiplier) => AttackDamage *= multiplier;

        public void ModifyAttackCooldown(float multiplier) => AttackCooldown *= multiplier;

        public void ModifyAttackRange(float multiplier) => AttackRange *= multiplier;

        public void ModifySpeed(float multiplier) => Speed *= multiplier;

        public void ModifyMaxHealth(float multiplier)
        {
            var oldMaxHealth = MaxHealth;
            MaxHealth *= multiplier;
            Health += MaxHealth - oldMaxHealth;
        }

        public void AddVampirismPercent(float value) => VampirismPercent += value;

        public void AddExperienceMultiplier(float value) => ExperienceMultiplier += value;
        
        private void CalculateNextLevelExp()
        {
            ExperienceToNextLevel = 100f * (float)Math.Pow(Level, 1.5);
        }
    }
}
