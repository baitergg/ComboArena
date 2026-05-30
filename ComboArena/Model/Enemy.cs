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
        /// <summary>Красный враг — лёгкий, быстрый, слабый.</summary>
        Red,

        /// <summary>Синий враг — средний, выносливый, с рывком.</summary>
        Blue,

        /// <summary>Жёлтый враг — тяжёлый, медленный, с лазером.</summary>
        Yellow
    }

    /// <summary>
    /// Уровни сложности врагов. С каждым тиром враги получают новые способности.
    /// Normal (1-2) → Elite (3-5) → Champion (6-8) → Boss (9+).
    /// </summary>
    public enum EnemyTier
    {
        /// <summary>Обычный враг (уровни 1-2). Без способностей.</summary>
        Normal = 0,

        /// <summary>Элитный враг (уровни 3-5). Получает первую способность.</summary>
        Elite = 1,

        /// <summary>Чемпион (уровни 6-8). Улучшенная способность.</summary>
        Champion = 2,

        /// <summary>Босс (уровень 9+). Максимальная сила и способности.</summary>
        Boss = 3
    }

    /// <summary>
    /// Базовый класс для всех врагов.
    /// Содержит характеристики (урон, здоровье, скорость), способности
    /// (ярость, рывок, лазер) и логику движения к игроку.
    /// </summary>
    public abstract class Enemy : Entity
    {
        /// <summary>Тип врага (Red/Blue/Yellow).</summary>
        public abstract EnemyType Type { get; }
        
        /// <summary>Урон, наносимый врагом при контакте.</summary>
        public float Damage { get; private set; }

        /// <summary>Количество опыта, даваемое за убийство.</summary>
        public float ExperienceReward { get; private set; }

        /// <summary>Тир сложности врага.</summary>
        public EnemyTier Tier { get; private set; } = EnemyTier.Normal;

        /// <summary>Уровень врага (соответствует уровню игрока на момент спавна).</summary>
        public int Level { get; private set; } = 1;
        
        /// <summary>Базовый урон (до масштабирования).</summary>
        protected float BaseDamage { get; }

        /// <summary>Базовый кулдаун атаки в секундах.</summary>
        protected float BaseAttackCooldown { get; }

        /// <summary>Базовая дистанция обнаружения игрока в пикселях.</summary>
        protected float BaseDetectionRange { get; }

        /// <summary>Базовая награда опытом.</summary>
        protected float BaseExperienceReward { get; }

        /// <summary>Базовое максимальное здоровье.</summary>
        protected float BaseMaxHealth { get; }

        /// <summary>Базовая скорость движения.</summary>
        protected float BaseSpeed { get; }
        
        /// <summary>Кулдаун между атаками в секундах.</summary>
        private float AttackCooldown { get; set; }

        /// <summary>Дистанция обнаружения игрока в пикселях.</summary>
        private float DetectionRange { get; set; }

        /// <summary>Таймер до следующей атаки.</summary>
        private float _attackTimer;

        /// <summary>Генератор случайных чисел.</summary>
        protected readonly Random _random = new();

        /// <summary>Текущее направление движения.</summary>
        private Vector2 _targetDirection;

        /// <summary>Таймер смены случайного направления (когда игрок вне зоны обнаружения).</summary>
        private float _directionChangeTimer;

        /// <summary>Ссылка на игрока — цель врага.</summary>
        protected Player _playerTarget;
        
        /// <summary>Может ли враг делать рывок (Blue).</summary>
        public bool CanRush { get; private set; }

        /// <summary>Может ли враг стрелять лазером (Yellow).</summary>
        public bool CanShootLaser { get; private set; }

        /// <summary>В ярости ли враг (Red) — ускорение при низком HP.</summary>
        public bool IsEnraged { get; private set; }
        
        /// <summary>Кулдаун между рывками.</summary>
        public float RushCooldown { get; private set; }

        /// <summary>Скорость во время рывка.</summary>
        public float RushSpeed { get; private set; }

        /// <summary>Урон лазера.</summary>
        public float LaserDamage { get; private set; }

        /// <summary>Дальность лазера.</summary>
        public float LaserRange { get; private set; }

        /// <summary>Множитель скорости в режиме ярости.</summary>
        public float EnrageSpeedMultiplier { get; private set; }
        
        /// <summary>Таймеры способностей.</summary>
        private float _rushCooldownTimer;
        private float _rushDurationTimer;
        private bool _isRushing;
        private Vector2 _rushDirection;
        private const float RushDuration = 0.3f;

        private float _laserCooldownTimer;
        private float _laserDurationTimer;
        private bool _laserHitApplied;

        /// <summary>true, если враг в настоящий момент стреляет лазером.</summary>
        public bool IsShootingLaser { get; private set; }

        /// <summary>Таймер анимации лазера.</summary>
        public float LaserAnimationTimer { get; private set; }

        /// <summary>Направление лазера.</summary>
        public Vector2 LaserDirection { get; private set; }

        private const float LaserDuration = 0.4f;
        
        /// <summary>
        /// Создаёт врага с заданными базовыми характеристиками.
        /// </summary>
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
        
        /// <summary>
        /// Масштабирует характеристики врага под уровень игрока.
        /// Вызывается при спавне. Определяет тир и разблокирует способности.
        /// </summary>
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

        /// <summary>
        /// Увеличивает максимальное HP существующего врага (каждые 3 уровня игрока).
        /// Сохраняет процент текущего здоровья.
        /// </summary>
        public void ApplyHpMultiplier(float multiplier)
        {
            var oldMaxHealth = MaxHealth;
            MaxHealth = (float)Math.Round(MaxHealth * multiplier);
            Health = (float)Math.Round(Health * (MaxHealth / oldMaxHealth));
        }

        /// <summary>
        /// Разблокирует способности в зависимости от тира.
        /// Переопределяется в конкретных классах врагов.
        /// </summary>
        protected virtual void UnlockAbilities()
        {
        }

        /// <summary>
        /// Устанавливает игрока как цель для преследования.
        /// </summary>
        /// <param name="player">Игрок.</param>
        public void SetPlayerTarget(Player player)
        {
            _playerTarget = player;
        }

        /// <summary>
        /// Обновляет состояние врага каждый кадр:
        /// движение к игроку, использование способностей, обновление таймеров.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
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

        /// <summary>
        /// Обновляет состояние способностей врага:
        /// ярость (Red), рывок (Blue), лазер (Yellow).
        /// </summary>
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

        /// <summary>
        /// Проверяет, попадает ли игрок в луч лазера.
        /// </summary>
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

        /// <summary>
        /// Пытается применить урон от лазера к игроку.
        /// Урон наносится только один раз за выстрел.
        /// </summary>
        public bool TryApplyLaserHit()
        {
            if (!IsShootingLaser || _laserHitApplied) return false;
            _laserHitApplied = true;
            return true;
        }

        /// <summary>
        /// Возвращает урон лазера.
        /// </summary>
        public float GetLaserDamage()
        {
            return LaserDamage;
        }

        /// <summary>
        /// Инициализирует способность рывка (Blue).
        /// </summary>
        protected void InitRushAbility(float cooldown, float speed)
        {
            CanRush = true;
            RushCooldown = cooldown;
            RushSpeed = speed;
            _rushCooldownTimer = cooldown * 0.5f;
        }

        /// <summary>
        /// Инициализирует способность лазера (Yellow).
        /// </summary>
        protected void InitLaserAbility(float damage, float range)
        {
            CanShootLaser = true;
            LaserDamage = damage;
            LaserRange = range;
            _laserCooldownTimer = 1.5f;
        }

        /// <summary>
        /// Инициализирует способность ярости (Red).
        /// </summary>
        protected void InitEnrageAbility(float speedMultiplier)
        {
            IsEnraged = true;
            EnrageSpeedMultiplier = speedMultiplier;
        }

        /// <summary>
        /// Проверяет, может ли враг атаковать (кулдаун прошёл).
        /// </summary>
        /// <returns>true, если атака доступна.</returns>
        public bool CanAttack()
        {
            return _attackTimer <= 0;
        }

        /// <summary>
        /// Сбрасывает таймер атаки (ставит кулдаун).
        /// </summary>
        public void ResetAttackCooldown()
        {
            _attackTimer = AttackCooldown;
        }

        /// <summary>
        /// Наносит урон врагу. Если враг умирает — публикует EnemyDeathEvent.
        /// </summary>
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
