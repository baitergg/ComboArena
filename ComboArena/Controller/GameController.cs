using ComboArena.Core;
using ComboArena.Model;
using ComboArena.Model.Abilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComboArena.Controller
{
    /// <summary>
    /// Главный контроллер игры. Управляет всеми игровыми процессами:
    /// игроком, врагами, выпадающими предметами, перками,
    /// обработкой ввода, коллизиями и спавном врагов.
    /// </summary>
    public class GameController
    {
        /// <summary>Игрок - главный управляемый персонаж.</summary>
        public Player Player { get; }

        /// <summary>Список всех активных врагов на карте.</summary>
        public List<Enemy> Enemies { get; }

        /// <summary>Список выпавших предметов (опыт/здоровье).</summary>
        public List<DropItem> DropItems { get; }

        /// <summary>true, если игрок в настоящий момент выбирает перк.</summary>
        public bool IsChoosingPerk { get; private set; }

        /// <summary>Список перков, предлагаемых игроку для выбора.</summary>
        public List<Perk> OfferedPerks { get; }

        /// <summary>Общий список всех сущностей.</summary>
        private List<Entity> _entities { get; }

        /// <summary>Шина событий.</summary>
        private readonly EventBus _eventBus;

        /// <summary>Количество перков, предлагаемых на выбор (всегда 3).</summary>
        private const int PerksToOffer = 3;

        /// <summary>Генератор случайных чисел.</summary>
        private readonly Random _random = new();

        /// <summary>Интервал между попытками спавна врагов (сек).</summary>
        private const float SpawnInterval = 1f;

        /// <summary>Ширина зоны спавна вокруг игрока.</summary>
        private const float SpawnZoneWidth = 1200f;

        /// <summary>Высота зоны спавна вокруг игрока.</summary>
        private const float SpawnZoneHeight = 800f;

        /// <summary>Минимальная дистанция от игрока для спавна врага.</summary>
        private const float MinSpawnDistance = 300f;

        /// <summary>Максимальное количество врагов на карте.</summary>
        private const int MaxEnemies = 30;

        /// <summary>Максимальная дистанция от игрока, после которой враг удаляется.</summary>
        private const float MaxEnemyDistance = 2000f;

        /// <summary>Максимальное количество попыток найти позицию для спавна.</summary>
        private const int MaxSpawnAttempts = 5;

        /// <summary>Таймер до следующего спавна врага.</summary>
        private float _spawnTimer;

        /// <summary>Множитель HP врагов (увеличивается каждые 3 уровня игрока).</summary>
        private float _enemyHpMultiplier = 1f;

        /// <summary>Флаг для обработки однократного нажатия клавиши способности.</summary>
        private bool _abilityKeyPressed;

        
        /// <summary>
        /// Создаёт контроллер игры, инициализирует игрока, списки врагов,
        /// дропа и перков. Подписывается на событие повышения уровня игрока
        /// и на событие смерти врага через EventBus.
        /// </summary>
        public GameController()
        {
            _eventBus = EventBus.Instance;
            Enemies = [];
            _entities = [];
            DropItems = [];
            OfferedPerks = [];
            Player = new Player(0, 0);
            _entities.Add(Player);
            Player.OnLevelUp += OnPlayerLevelUp;
            _eventBus.Subscribe("EnemyDeath", OnEnemyDeath);
        }
        
        /// <summary>
        /// Выбирает перк по индексу (0-2) и применяет его к игроку.
        /// </summary>
        /// <param name="index">Индекс выбранного перка.</param>
        public void SelectPerk(int index)
        {
            if (!IsChoosingPerk || index < 0 || index >= OfferedPerks.Count) return;

            var selectedPerk = OfferedPerks[index];
            Player.ApplyPerk(selectedPerk);
            IsChoosingPerk = false;
            OfferedPerks.Clear();
            _eventBus.Publish(new PerkSelectionEvent(false, OfferedPerks));
        }

        /// <summary>
        /// Главный метод обновления, вызываемый каждый кадр.
        /// Если игрок выбирает перк — обновление приостанавливается.
        /// Иначе: обработка ввода, обновление игрока, врагов, спавн, коллизии, дроп.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
        public void Update(GameTime gameTime)
        {
            if (IsChoosingPerk) return;

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ProcessInput();
            Player.Update(gameTime);
            Player.UpdateAbility(delta, Enemies);
            UpdateEnemies(gameTime);
            UpdateEnemySpawning(delta);
            ProcessCollisions();
            UpdateDropItems(gameTime);
        }
        
        /// <summary>
        /// Обрабатывает ввод с клавиатуры: движение (WASD), атаку (Space),
        /// активацию способности (E).
        /// </summary>
        private void ProcessInput()
        {
            var keyboardState = Keyboard.GetState();
            var direction = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W)) direction.Y = -1;
            if (keyboardState.IsKeyDown(Keys.S)) direction.Y = 1;
            if (keyboardState.IsKeyDown(Keys.A)) direction.X = -1;
            if (keyboardState.IsKeyDown(Keys.D)) direction.X = 1;

            Player.SetMovementDirection(direction);
            Player.SetAttacking(keyboardState.IsKeyDown(Keys.Space));

            // Активация способности по нажатию E 
            if (keyboardState.IsKeyDown(Keys.E) && !_abilityKeyPressed)
            {
                Player.UseAbility(Enemies);
                _abilityKeyPressed = true;
            }
            else if (!keyboardState.IsKeyDown(Keys.E))
            {
                _abilityKeyPressed = false;
            }
        }
        
        /// <summary>
        /// Обновляет всех врагов: движение, удаление мёртвых/далёких, создание дропа.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
        private void UpdateEnemies(GameTime gameTime)
        {
            for (var i = Enemies.Count - 1; i >= 0; i--)
            {
                var enemy = Enemies[i];
                enemy.Update(gameTime);

                // Удаляем врагов, ушедших слишком далеко от игрока
                var distance = Vector2.Distance(enemy.Position, Player.Position);
                if (distance > MaxEnemyDistance)
                {
                    Enemies.RemoveAt(i);
                    _entities.Remove(enemy);
                    continue;
                }

                // Если враг мёртв — удаляем (дроп и опыт создаются через EnemyDeathEvent)
                if (enemy.IsAlive) continue;

                Enemies.RemoveAt(i);
                _entities.Remove(enemy);
            }
        }

        /// <summary>
        /// Обновляет таймер спавна врагов и пытается создать нового врага.
        /// </summary>
        /// <param name="delta">Разница времени.</param>
        private void UpdateEnemySpawning(float delta)
        {
            _spawnTimer -= delta;
            if (_spawnTimer > 0) return;

            TrySpawnEnemy();
            _spawnTimer = SpawnInterval;
        }
        
        /// <summary>
        /// Создаёт врага случайного типа в указанной позиции.
        /// </summary>
        /// <returns>Новый экземпляр врага.</returns>
        private Enemy CreateRandomEnemy(float x, float y)
        {
            var enemyTypes = new[] { EnemyType.Red, EnemyType.Blue, EnemyType.Yellow };
            var randomType = enemyTypes[_random.Next(enemyTypes.Length)];
            return CreateEnemy(randomType, x, y);
        }

        /// <summary>
        /// Создаёт врага указанного типа в заданной позиции.
        /// </summary>
        /// <param name="type">Тип врага.</param>
        /// <returns>Новый экземпляр врага.</returns>
        private Enemy CreateEnemy(EnemyType type, float x, float y)
        {
            return type switch
            {
                EnemyType.Red => new RedEnemy(x, y),
                EnemyType.Blue => new BlueEnemy(x, y),
                EnemyType.Yellow => new YellowEnemy(x, y),
                _ => new RedEnemy(x, y)
            };
        }

        /// <summary>
        /// Пытается заспавнить врага в зоне вокруг игрока.
        /// Делает несколько попыток найти свободную позицию.
        /// </summary>
        private void TrySpawnEnemy()
        {
            if (Enemies.Count >= MaxEnemies) return;

            var playerCenter = Player.Position + new Vector2(Player.Width / 2, Player.Height / 2);
            var spawnHalfWidth = SpawnZoneWidth / 2;
            var spawnHalfHeight = SpawnZoneHeight / 2;

            for (var attempt = 0; attempt < MaxSpawnAttempts; attempt++)
            {
                var x = playerCenter.X + ((float)_random.NextDouble() * SpawnZoneWidth - spawnHalfWidth);
                var y = playerCenter.Y + ((float)_random.NextDouble() * SpawnZoneHeight - spawnHalfHeight);

                var dx = x - playerCenter.X;
                var dy = y - playerCenter.Y;
                var distance = (float)Math.Sqrt(dx * dx + dy * dy);

                // Проверяем минимальную дистанцию от игрока
                if (distance < MinSpawnDistance) continue;

                var enemy = CreateRandomEnemy(x, y);
                enemy.SetPlayerTarget(Player);
                enemy.ApplyDifficultyScaling(Player.Level, _enemyHpMultiplier);

                Enemies.Add(enemy);
                _entities.Add(enemy);

                return;
            }
        }
        
        /// <summary>
        /// Обрабатывает все коллизии: подбор дропа, лазерные атаки,
        /// контактный урон врагов, атаку игрока по врагам.
        /// </summary>
        private void ProcessCollisions()
        {
            if (!Player.IsAlive) return;

            // Сначала собираем дропы, чтобы новые дропы не были подобраны в том же кадре
            CollectDrops();

            // Обработка лазеров Yellow-врагов
            ProcessLaserAttacks();

            // Контактный урон от врагов
            foreach (var enemy in Enemies)
            {
                var collides = enemy.CollidesWith(Player);
                if (!collides || !enemy.CanAttack()) continue;

                var damage = enemy.Damage;
                var oldHealth = Player.Health;
                Player.TakeDamage(damage);

                if (Player.Health < oldHealth)
                {
                    enemy.ResetAttackCooldown();
                }
            }

            // Атака игрока по врагам
            if (Player.IsAttacking)
            {
                for (var i = Enemies.Count - 1; i >= 0; i--)
                {
                    var enemy = Enemies[i];
                    if (!Player.IsInAttackArc(enemy)) continue;

                    var damage = Player.AttackDamage * Player.GetComboBonusDamage(1f);
                    enemy.TakeDamage(damage);

                    // Дроп, опыт, комбо и вампиризм создаются через EnemyDeathEvent
                    if (!enemy.IsAlive)
                    {
                        Enemies.RemoveAt(i);
                        _entities.Remove(enemy);
                    }
                }

                Player.ResetAttackCooldown();
            }
        }

        /// <summary>
        /// Обрабатывает лазерные атаки Yellow-врагов по игроку.
        /// </summary>
        private void ProcessLaserAttacks()
        {
            foreach (var enemy in Enemies)
            {
                if (!enemy.IsAlive) continue;
                if (!enemy.IsShootingLaser) continue;
                if (!enemy.IsPlayerInLaser(Player)) continue;
                if (!enemy.TryApplyLaserHit()) continue;

                var laserDamage = enemy.GetLaserDamage();
                Player.TakeDamage(laserDamage);
            }
        }

        /// <summary>
        /// Собирает все дропы, с которыми пересекается игрок.
        /// </summary>
        private void CollectDrops()
        {
            for (var i = DropItems.Count - 1; i >= 0; i--)
            {
                var drop = DropItems[i];
                if (!drop.CollidesWith(Player)) continue;

                drop.Collect(Player);
                DropItems.RemoveAt(i);
            }
        }

        /// <summary>
        /// Обновляет все выпавшие предметы и удаляет истёкшие.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
        private void UpdateDropItems(GameTime gameTime)
        {
            for (var i = DropItems.Count - 1; i >= 0; i--)
            {
                var drop = DropItems[i];
                drop.Update(gameTime);

                if (!drop.IsActive)
                {
                    DropItems.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Создаёт выпадающие предметы в позиции врага.
        /// 30% шанс выпадения сердца (здоровье) + 1-3 частицы опыта.
        /// </summary>
        /// <param name="position">Позиция, где был убит враг.</param>
        /// <param name="experienceReward">Базовая награда опытом врага.</param>
        private void CreateDrop(Vector2 position, float experienceReward)
        {
            // 30% шанс выпадения сердца
            if (_random.NextDouble() < 0.3)
            {
                var healthValue = Player.MaxHealth * 0.15f;
                var healthDrop = new DropItem(position, DropType.Health, healthValue);
                DropItems.Add(healthDrop);
            }

            // 1-3 частицы опыта
            var expParticles = _random.Next(1, 4);
            for (var i = 0; i < expParticles; i++)
            {
                var offset = new Vector2(
                    (float)(_random.NextDouble() * 40 - 20),
                    (float)(_random.NextDouble() * 40 - 20));

                var expValue = experienceReward / expParticles;
                var expDrop = new DropItem(position + offset, DropType.Experience, expValue);
                DropItems.Add(expDrop);
            }
        }
        
        /// <summary>
        /// Начинает выбор перка. Гарантирует, что один из трёх слотов —
        /// способность или её улучшение. Остальные слоты заполняются
        /// случайными базовыми перками.
        /// </summary>
        private void StartPerkSelection()
        {
            OfferedPerks.Clear();

            var basePerkTypes = new List<PerkType>
            {
                PerkType.DamageUp,
                PerkType.AttackSpeedUp,
                PerkType.MaxHealthUp,
                PerkType.SpeedUp,
                PerkType.RangeUp,
                PerkType.Vampirism,
                PerkType.ExperienceBoost
            };

            // Если у игрока ещё нет способности — предлагаем одну из двух
            if (!Player.HasAbility)
            {
                var abilityChoice = _random.Next(2) == 0
                    ? PerkType.AbilityFireBurst
                    : PerkType.AbilityBarrier;

                OfferedPerks.Add(new Perk(abilityChoice));

                var otherAbility = abilityChoice == PerkType.AbilityFireBurst
                    ? PerkType.AbilityBarrier
                    : PerkType.AbilityFireBurst;
                basePerkTypes.Add(otherAbility);
            }
            else if (Player.Ability is FireBurst)
            {
                // Если есть FireBurst — предлагаем улучшение для него
                var upgrades = new List<PerkType>
                {
                    PerkType.FireBurstRadiusUp,
                    PerkType.FireBurstDamageUp,
                    PerkType.FireBurstCooldownDown
                };

                var chosenIndex = _random.Next(upgrades.Count);
                OfferedPerks.Add(new Perk(upgrades[chosenIndex]));
                upgrades.RemoveAt(chosenIndex);
                basePerkTypes.AddRange(upgrades);
            }
            else if (Player.Ability is Barrier)
            {
                // Если есть Barrier — предлагаем улучшение для него
                var upgrades = new List<PerkType>
                {
                    PerkType.BarrierDurationUp,
                    PerkType.BarrierCooldownDown
                };

                var chosenIndex = _random.Next(upgrades.Count);
                OfferedPerks.Add(new Perk(upgrades[chosenIndex]));
                upgrades.RemoveAt(chosenIndex);
                basePerkTypes.AddRange(upgrades);
            }

            // Заполняем оставшиеся слоты случайными базовыми перками
            var pool = basePerkTypes.OrderBy(_ => _random.Next()).ToList();
            foreach (var perkType in pool)
            {
                if (OfferedPerks.Count >= PerksToOffer) break;
                OfferedPerks.Add(new Perk(perkType));
            }

            IsChoosingPerk = true;
            _eventBus.Publish(new PerkSelectionEvent(true, OfferedPerks));
        }
        
        /// <summary>
        /// Обработчик смерти врага. Создаёт дроп, начисляет опыт,
        /// увеличивает комбо и применяет вампиризм.
        /// </summary>
        private void OnEnemyDeath(IEvent evt)
        {
            if (evt is not EnemyDeathEvent deathEvent) return;

            CreateDrop(deathEvent.Position, deathEvent.ExperienceReward);
            Player.AddExperience(deathEvent.ExperienceReward * Player.GetComboBonusExperience(1f));
            Player.AddCombo();
            Player.HealOnKill();
        }

        /// <summary>
        /// Обработчик повышения уровня игрока.
        /// Каждые 3 уровня: увеличивает HP врагов, запускает выбор перка.
        /// </summary>
        /// <param name="newLevel">Новый уровень игрока.</param>
        private void OnPlayerLevelUp(int newLevel)
        {
            if (newLevel % 3 == 0)
            {
                _enemyHpMultiplier *= 1.5f;

                foreach (var enemy in Enemies)
                {
                    enemy.ApplyHpMultiplier(1.5f);
                }

                StartPerkSelection();
            }
        }
    }
}
