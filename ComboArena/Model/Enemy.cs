using ComboArena.Core;
using Microsoft.Xna.Framework;
using System;

namespace ComboArena.Model
{
    /// <summary>
    /// Тип врага, определяющий его визуальное отображение и базовую роль.
    /// </summary>
    public enum EnemyType
    {
        Red,

        Blue,

        Yellow
    }

    /// <summary>
    /// Уровни сложности врагов. С каждым тиром враги получают новые способности.
    /// Normal (1-2) > Elite (3-5) > Champion (6-8) > Boss (9+).
    /// </summary>
    public enum EnemyTier
    {
        Normal = 0,

        Elite = 1,

        Champion = 2,

        Boss = 3
    }

    /// <summary>
    /// Базовый класс для всех врагов.
    /// Содержит характеристики (урон, здоровье, скорость), способности
    /// (ярость, рывок, лазер) и логику движения к игроку.
    /// </summary>
    public abstract class Enemy : Entity
    {
        public abstract EnemyType Type { get; }
        
        public float Damage { get; private set; }

        private float ExperienceReward { get; set; }

        public EnemyTier Tier { get; private set; } = EnemyTier.Normal;

        public int Level { get; private set; } = 1;

        private float BaseDamage { get; }

        private float BaseAttackCooldown { get; }

        private float BaseDetectionRange { get; }

        private float BaseExperienceReward { get; }

        private float BaseMaxHealth { get; }

        protected float BaseSpeed { get; }
        
        private float AttackCooldown { get; set; }

        private float DetectionRange { get; set; }

        private float _attackTimer;

        private readonly Random _random = new();

        private Vector2 _targetDirection;

        private float _directionChangeTimer;

        private Player _playerTarget;

        private bool CanRush { get; set; }

        private bool CanShootLaser { get; set; }

        private bool IsEnraged { get; set; }

        private float RushCooldown { get; set; }

        private float RushSpeed { get; set; }

        private float LaserDamage { get; set; }

        public float LaserRange { get; private set; }

        private float EnrageSpeedMultiplier { get; set; }
        
        private float _rushCooldownTimer;
        private float _rushDurationTimer;
        private bool _isRushing;
        private Vector2 _rushDirection;
        private const float RushDuration = 0.3f;

        private float _laserCooldownTimer;
        private float _laserDurationTimer;
        private bool _laserHitApplied;

        public bool IsShootingLaser { get; private set; }

        public float LaserAnimationTimer { get; private set; }

        public Vector2 LaserDirection { get; private set; }

        private const float LaserDuration = 0.4f;
        
        protected Enemy(float x, float y, float width, float height, float maxHealth, float speed,
            float damage, float attackCooldown, float detectionRange, float experienceReward)
            : base(x, y, width, height, maxHealth, speed)
        {
            BaseMaxHealth = maxHealth;
            BaseSpeed = speed;
            BaseDamage = damage;
            BaseAttackCooldown = attackCooldown;
            BaseDetectionRange = detectionRange;
            BaseExperienceReward = experienceReward;

            Damage = damage;
            AttackCooldown = attackCooldown;
            DetectionRange = detectionRange;
            ExperienceReward = experienceReward;

            _targetDirection = new Vector2((float)_random.NextDouble() * 2 - 1, (float)_random.NextDouble() * 2 - 1);
            if (_targetDirection.LengthSquared() > 0)
                _targetDirection.Normalize();
            CollisionScale = 0.35f;
        }
        
        public void ApplyDifficultyScaling(int playerLevel, float hpScalingMultiplier = 1f)
        {
            Level = playerLevel;

            // Определение тира по уровню
            if (playerLevel >= 9) Tier = EnemyTier.Boss;
            else if (playerLevel >= 6) Tier = EnemyTier.Champion;
            else if (playerLevel >= 3) Tier = EnemyTier.Elite;
            else Tier = EnemyTier.Normal;

            // Расчёт множителей
            var healthMultiplier = (1f + (playerLevel - 1) * 0.15f) * hpScalingMultiplier;
            var damageMultiplier = 1f + (playerLevel - 1) * 0.1f;
            var speedMultiplier = 1f + (playerLevel - 1) * 0.03f;
            var expMultiplier = 1f + (playerLevel - 1) * 0.2f;

            // Применение масштабирования
            MaxHealth = (float)Math.Round(BaseMaxHealth * healthMultiplier);
            Health = MaxHealth;
            Damage = (float)Math.Round(BaseDamage * damageMultiplier);
            Speed = BaseSpeed * speedMultiplier;
            ExperienceReward = (float)Math.Round(BaseExperienceReward * expMultiplier);

            AttackCooldown = Math.Max(0.3f, BaseAttackCooldown * (1f - (playerLevel - 1) * 0.03f));
            DetectionRange = BaseDetectionRange * (1f + (playerLevel - 1) * 0.05f);

            UnlockAbilities();
        }

        public void ApplyHpMultiplier(float multiplier)
        {
            var oldMaxHealth = MaxHealth;
            MaxHealth = (float)Math.Round(MaxHealth * multiplier);
            Health = (float)Math.Round(Health * (MaxHealth / oldMaxHealth));
        }

        protected virtual void UnlockAbilities()
        {
        }

        public void SetPlayerTarget(Player player)
        {
            _playerTarget = player;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_playerTarget != null && _playerTarget.IsAlive)
            {
                var toPlayer = _playerTarget.Position - Position;
                var distance = toPlayer.Length();

                UpdateAbilities(delta, toPlayer, distance);

                if (distance <= DetectionRange)
                {
                    // Преследование игрока
                    if (toPlayer.LengthSquared() > 0)
                    {
                        _targetDirection = toPlayer;
                        _targetDirection.Normalize();
                    }
                }
                else
                {
                    // Случайное блуждание
                    _directionChangeTimer -= delta;
                    if (_directionChangeTimer <= 0)
                    {
                        _targetDirection = new Vector2(
                            (float)_random.NextDouble() * 2 - 1,
                            (float)_random.NextDouble() * 2 - 1);
                        if (_targetDirection.LengthSquared() > 0)
                            _targetDirection.Normalize();
                        _directionChangeTimer = _random.Next(1, 3);
                    }
                }
            }

            // Применение скорости (рывок или обычное движение)
            if (_isRushing)
            {
                Velocity = _rushDirection * RushSpeed;
            }
            else
            {
                Velocity = _targetDirection * Speed;
            }

            // Обновление таймера атаки
            if (_attackTimer > 0)
            {
                _attackTimer -= delta;
            }
        }

        private void UpdateAbilities(float delta, Vector2 toPlayer, float distance)
        {
            if (IsEnraged && Health / MaxHealth < 0.3f)
            {
                Speed = BaseSpeed * EnrageSpeedMultiplier * (1f + (Level - 1) * 0.02f);
            }

            if (CanRush)
            {
                _rushCooldownTimer -= delta;

                if (!_isRushing && _rushCooldownTimer <= 0 && distance > 80 && distance < DetectionRange * 0.9f)
                {
                    _rushDirection = toPlayer;
                    if (_rushDirection.LengthSquared() > 0)
                        _rushDirection.Normalize();
                    _isRushing = true;
                    _rushDurationTimer = RushDuration;
                    _rushCooldownTimer = RushCooldown;
                }
            }

            if (_isRushing)
            {
                _rushDurationTimer -= delta;
                if (_rushDurationTimer <= 0)
                {
                    _isRushing = false;
                }
            }

            if (CanShootLaser)
            {
                if (IsShootingLaser)
                {
                    _laserDurationTimer -= delta;
                    LaserAnimationTimer = _laserDurationTimer;

                    if (_laserDurationTimer <= 0)
                    {
                        IsShootingLaser = false;
                        _laserHitApplied = false;
                    }
                }
                else
                {
                    _laserCooldownTimer -= delta;

                    if (distance <= LaserRange && _laserCooldownTimer <= 0)
                    {
                        IsShootingLaser = true;
                        _laserDurationTimer = LaserDuration;
                        LaserAnimationTimer = LaserDuration;
                        _laserHitApplied = false;
                        LaserDirection = toPlayer;
                        if (LaserDirection.LengthSquared() > 0)
                            LaserDirection.Normalize();
                        _laserCooldownTimer = 2.5f - Math.Min(Level * 0.1f, 1.0f);
                    }
                }
            }
        }

        public bool IsPlayerInLaser(Player player)
        {
            var enemyCenter = Position + new Vector2(Width / 2, Height / 2);
            var playerCenter = player.Position + new Vector2(player.Width / 2, player.Height / 2);
            var toPlayer = playerCenter - enemyCenter;
            var distance = toPlayer.Length();

            if (distance > LaserRange) return false;
            if (distance == 0) return false;

            toPlayer.Normalize();
            var dot = Vector2.Dot(LaserDirection, toPlayer);
            var angle = (float)Math.Acos(dot);
            const float laserArc = 0.15f;

            return Math.Abs(angle) <= laserArc;
        }

        public bool TryApplyLaserHit()
        {
            if (!IsShootingLaser || _laserHitApplied) return false;
            _laserHitApplied = true;
            return true;
        }

        public float GetLaserDamage()
        {
            return LaserDamage;
        }

        protected void InitRushAbility(float cooldown, float speed)
        {
            CanRush = true;
            RushCooldown = cooldown;
            RushSpeed = speed;
            _rushCooldownTimer = cooldown * 0.5f;
        }

        protected void InitLaserAbility(float damage, float range)
        {
            CanShootLaser = true;
            LaserDamage = damage;
            LaserRange = range;
            _laserCooldownTimer = 1.5f;
        }

        protected void InitEnrageAbility(float speedMultiplier)
        {
            IsEnraged = true;
            EnrageSpeedMultiplier = speedMultiplier;
        }

        public bool CanAttack()
        {
            return _attackTimer <= 0;
        }

        public void ResetAttackCooldown()
        {
            _attackTimer = AttackCooldown;
        }

        public override void TakeDamage(float damage)
        {
            var oldHealth = Health;
            base.TakeDamage(damage);

            if (!IsAlive && oldHealth > 0)
            {
                EventBus.Instance.Publish(new EnemyDeathEvent(this, Position, ExperienceReward));
            }
        }
    }
}
