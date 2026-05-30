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
        /// <summary>Время в секундах, через которое комбо сбрасывается.</summary>
        public const float ComboTimeout = 3f;
        
        /// <summary>Базовый урон атаки.</summary>
        public float AttackDamage { get; private set; } = 10f;

        /// <summary>Дальность атаки в пикселях.</summary>
        public float AttackRange { get; private set; } = 70f;

        /// <summary>Кулдаун между атаками в секундах.</summary>
        public float AttackCooldown { get; private set; } = 0.5f;
        
        /// <summary>Направление, в которое смотрит игрок (вправо/влево).</summary>
        public Vector2 FacingDirection { get; private set; }

        /// <summary>true, если игрок в настоящий момент совершает атаку.</summary>
        public bool IsAttacking { get; private set; }
        
        /// <summary>Текущий уровень игрока.</summary>
        public int Level { get; private set; } = 1;

        /// <summary>Текущее количество опыта.</summary>
        public float Experience { get; private set; }

        /// <summary>Опыта, необходимо для следующего уровня.</summary>
        public float ExperienceToNextLevel { get; private set; } = 100f;

        /// <summary>Событие, вызываемое при повышении уровня.</summary>
        public event Action<int> OnLevelUp;

        /// <summary>Процент вампиризма (0.1 = 10% от урона восстанавливается как HP).</summary>
        public float VampirismPercent { get; private set; }

        /// <summary>Множитель получаемого опыта.</summary>
        public float ExperienceMultiplier { get; private set; } = 1f;

        /// <summary>Список активных перков, применённых к игроку.</summary>
        public List<Perk> ActivePerks { get; } = [];
        
        /// <summary>Текущее количество комбо-ударов подряд.</summary>
        public int ComboCount { get; private set; }

        /// <summary>Максимальное достигнутое комбо за сессию.</summary>
        public int MaxCombo { get; private set; }

        /// <summary>Множитель урона от комбо: 1 + (ComboCount * 0.05).</summary>
        public float ComboDamageMultiplier => 1f + (ComboCount * 0.05f);

        /// <summary>Множитель опыта от комбо: 1 + (ComboCount * 0.03).</summary>
        public float ComboExperienceMultiplier => 1f + (ComboCount * 0.03f);

        /// <summary>Оставшееся время до сброса комбо в секундах.</summary>
        public float ComboTimer { get; private set; }
        
        /// <summary>Текущая активная способность (может быть null).</summary>
        public ActiveAbility Ability { get; private set; }

        /// <summary>true, если у игрока есть активная способность.</summary>
        public bool HasAbility => Ability != null;

        /// <summary>true, если игрок неуязвим под действием щита.</summary>
        public bool IsInvulnerable { get; set; }
        
        /// <summary>Направление движения, установленное из ввода.</summary>
        private Vector2 _movementDirection = Vector2.Zero;

        /// <summary>Флаг: хочет ли игрок атаковать в этом кадре.</summary>
        private bool _wantToAttack;

        /// <summary>Таймер кулдауна атаки.</summary>
        private float _attackTimer;

        /// <summary>Таймер визуального отображения атаки.</summary>
        private float _attackVisualTimer;

        /// <summary>Длительность визуального эффекта атаки в секундах.</summary>
        private const float AttackVisualDuration = 0.3f;

        /// <summary>
        /// Создаёт игрока в указанной позиции с базовыми характеристиками.
        /// </summary>
        public Player(float x, float y)
            : base(x, y, width: 120, height: 100, maxHealth: 100, speed: 200)
        {
            FacingDirection = Vector2.UnitX;
            CalculateNextLevelExp();
            CollisionScale = 0.3f;
        }
        
        /// <summary>
        /// Устанавливает направление движения игрока.
        /// Вектор автоматически нормализуется.
        /// </summary>
        public void SetMovementDirection(Vector2 direction)
        {
            _movementDirection = direction;
            if (_movementDirection.LengthSquared() > 0)
            {
                _movementDirection.Normalize();
            }
        }

        /// <summary>
        /// Устанавливает флаг желания атаковать.
        /// </summary>
        public void SetAttacking(bool attacking)
        {
            _wantToAttack = attacking;
        }
        
        /// <summary>
        /// Обновляет состояние игрока каждый кадр:
        /// движение, атаку, таймеры и комбо-систему.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
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

            // Обработка кулдауна атаки
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
        
        /// <summary>
        /// Проверяет, находится ли указанная цель в прямоугольной зоне атаки перед игроком.
        /// </summary>
        public bool IsInAttackArc(Entity target)
        {
            return GetAttackBounds().Intersects(target.GetCollisionBounds());
        }

        /// <summary>
        /// Возвращает прямоугольник зоны атаки перед игроком (по направлению взгляда).
        /// </summary>
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
        
        /// <summary>
        /// Добавляет опыт игроку. При достижении порога — повышает уровень.
        /// Вызывает событие OnLevelUp при каждом повышении.
        /// </summary>
        /// <param name="amount">Базовое количество опыта (умножается на ExperienceMultiplier).</param>
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

        /// <summary>
        /// Восстанавливает здоровье при убийстве врага (вампиризм).
        /// </summary>
        public void HealOnKill()
        {
            var oldHealth = Health;
            var healAmount = MaxHealth * VampirismPercent;
            Health = Math.Min(Health + healAmount, MaxHealth);
            EventBus.Instance.Publish(new HealthChangedEvent(this, oldHealth, Health));
        }

        /// <summary>
        /// Наносит урон игроку. Если игрок неуязвим - урон игнорируется.
        /// При получении урона комбо сбрасывается.
        /// </summary>
        public override void TakeDamage(float damage)
        {
            if (IsInvulnerable) return;
            base.TakeDamage(damage);
            if (damage > 0)
            {
                ResetCombo();
            }
        }
        
        /// <summary>
        /// Применяет перк к игроку и добавляет его в список активных.
        /// </summary>
        public void ApplyPerk(Perk perk)
        {
            perk.Apply(this);
            ActivePerks.Add(perk);
        }


        /// <summary>
        /// Увеличивает счётчик комбо и сбрасывает таймер комбо.
        /// </summary>
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

        /// <summary>
        /// Сбрасывает комбо в ноль.
        /// </summary>
        public void ResetCombo()
        {
            ComboCount = 0;
            ComboTimer = 0f;
            EventBus.Instance.Publish(new ComboEvent(0, MaxCombo, 1f, 1f));
        }

        /// <summary>
        /// Сбрасывает таймер атаки (принудительно ставит кулдаун).
        /// </summary>
        public void ResetAttackCooldown()
        {
            _attackTimer = AttackCooldown;
        }


        /// <summary>
        /// Устанавливает активную способность игроку.
        /// </summary>
        public void SetAbility(ActiveAbility ability)
        {
            Ability = ability;
        }

        /// <summary>
        /// Активирует текущую способность, передавая ей список врагов.
        /// </summary>
        public void UseAbility(List<Enemy> enemies)
        {
            Ability?.Activate(this, enemies);
        }

        /// <summary>
        /// Обновляет состояние активной способности каждый кадр.
        /// </summary>
        public void UpdateAbility(float delta, List<Enemy> enemies)
        {
            Ability?.Update(delta, this, enemies);
        }

        /// <summary>
        /// Возвращает урон с учётом бонуса от комбо.
        /// </summary>
        public float GetComboBonusDamage(float baseDamage)
        {
            return baseDamage * ComboDamageMultiplier;
        }

        /// <summary>
        /// Возвращает опыт с учётом бонуса от комбо.
        /// </summary>
        public float GetComboBonusExperience(float baseExperience)
        {
            return baseExperience * ComboExperienceMultiplier;
        }

        /// <summary>Умножает урон атаки на множитель.</summary>
        public void ModifyAttackDamage(float multiplier) => AttackDamage *= multiplier;

        /// <summary>Умножает кулдаун атаки на множитель.</summary>
        public void ModifyAttackCooldown(float multiplier) => AttackCooldown *= multiplier;

        /// <summary>Умножает дальность атаки на множитель.</summary>
        public void ModifyAttackRange(float multiplier) => AttackRange *= multiplier;

        /// <summary>Умножает скорость передвижения на множитель.</summary>
        public void ModifySpeed(float multiplier) => Speed *= multiplier;

        /// <summary>
        /// Умножает максимальное здоровье на множитель.
        /// Текущее здоровье увеличивается пропорционально.
        /// </summary>
        public void ModifyMaxHealth(float multiplier)
        {
            var oldMaxHealth = MaxHealth;
            MaxHealth *= multiplier;
            Health += MaxHealth - oldMaxHealth;
        }

        /// <summary>Добавляет процент вампиризма.</summary>
        public void AddVampirismPercent(float value) => VampirismPercent += value;

        /// <summary>Добавляет множитель опыта.</summary>
        public void AddExperienceMultiplier(float value) => ExperienceMultiplier += value;
        
        /// <summary>
        /// Пересчитывает количество опыта, необходимое для следующего уровня.
        /// Формула: 100 * (Level ^ 1.5).
        /// </summary>
        private void CalculateNextLevelExp()
        {
            ExperienceToNextLevel = 100f * (float)Math.Pow(Level, 1.5);
        }
    }
}
