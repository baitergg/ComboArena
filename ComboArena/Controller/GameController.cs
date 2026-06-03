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
        public Player Player { get; }

        public List<Enemy> Enemies { get; }

        public List<DropItem> DropItems { get; }

        public bool IsChoosingPerk { get; private set; }

        public List<Perk> OfferedPerks { get; }

        private readonly EventBus _eventBus;

        private const int PerksToOffer = 3;

        private readonly Random _random = new();

        private const float SpawnInterval = 1f;

        private const float SpawnZoneWidth = 1200f;

        private const float SpawnZoneHeight = 800f;

        private const float MinSpawnDistance = 300f;

        private const int MaxEnemies = 30;

        private const float MaxEnemyDistance = 2000f;

        private const int MaxSpawnAttempts = 5;

        private float _spawnTimer;

        private float _enemyHpMultiplier = 1f;

        private bool _abilityKeyPressed;

        
        public GameController()
        {
            _eventBus = EventBus.Instance;
            Enemies = [];
            DropItems = [];
            OfferedPerks = [];
            Player = new Player(0, 0);
            Player.OnLevelUp += OnPlayerLevelUp;
            _eventBus.Subscribe("EnemyDeath", OnEnemyDeath);
        }
        
        public void SelectPerk(int index)
        {
            if (!IsChoosingPerk || index < 0 || index >= OfferedPerks.Count) return;

            var selectedPerk = OfferedPerks[index];

            // Если выбрана способность, а у игрока уже есть другая - удаляем старую
            if (selectedPerk.Type is PerkType.AbilityFireBurst or PerkType.AbilityBarrier)
            {
                if (Player.HasAbility)
                    Player.RemoveAbility();
            }

            Player.ApplyPerk(selectedPerk);
            Player.HealToFull();
            IsChoosingPerk = false;
            OfferedPerks.Clear();
            _eventBus.Publish(new PerkSelectionEvent(false, OfferedPerks));
        }

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
                    continue;
                }

                // Если враг мёртв - удаляем (дроп и опыт создаются через EnemyDeathEvent)
                if (enemy.IsAlive) continue;

                Enemies.RemoveAt(i);
            }
        }

        private void UpdateEnemySpawning(float delta)
        {
            _spawnTimer -= delta;
            if (_spawnTimer > 0) return;

            TrySpawnEnemy();
            _spawnTimer = SpawnInterval;
        }
        
        private Enemy CreateRandomEnemy(float x, float y)
        {
            var enemyTypes = new[] { EnemyType.Red, EnemyType.Blue, EnemyType.Yellow };
            var randomType = enemyTypes[_random.Next(enemyTypes.Length)];
            return CreateEnemy(randomType, x, y);
        }

        private static Enemy CreateEnemy(EnemyType type, float x, float y)
        {
            return type switch
            {
                EnemyType.Red => new RedEnemy(x, y),
                EnemyType.Blue => new BlueEnemy(x, y),
                EnemyType.Yellow => new YellowEnemy(x, y),
                _ => new RedEnemy(x, y)
            };
        }

        private void TrySpawnEnemy()
        {
            if (Enemies.Count >= MaxEnemies) return;

            var playerCenter = Player.Position + new Vector2(Player.Width / 2, Player.Height / 2);
            const float spawnHalfWidth = SpawnZoneWidth / 2;
            const float spawnHalfHeight = SpawnZoneHeight / 2;

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

                return;
            }
        }
        
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

                    var damage = Player.AttackDamage * Player.ComboDamageMultiplier;
                    enemy.TakeDamage(damage);

                    // Дроп, опыт, комбо и вампиризм создаются через EnemyDeathEvent
                    if (!enemy.IsAlive)
                    {
                        Enemies.RemoveAt(i);
                    }
                }

                Player.ResetAttackCooldown();
            }
        }

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

            if (!Player.HasAbility)
            {
                // Нет способности - предлагаем одну из двух случайно
                var abilityChoice = _random.Next(2) == 0
                    ? PerkType.AbilityFireBurst
                    : PerkType.AbilityBarrier;

                OfferedPerks.Add(new Perk(abilityChoice));

                var otherAbility = abilityChoice == PerkType.AbilityFireBurst
                    ? PerkType.AbilityBarrier
                    : PerkType.AbilityFireBurst;
                basePerkTypes.Add(otherAbility);
            }
            else
            {
                // Есть способность - предлагаем другую (не текущую)
                var otherAbility = Player.Ability is FireBurst
                    ? PerkType.AbilityBarrier
                    : PerkType.AbilityFireBurst;

                OfferedPerks.Add(new Perk(otherAbility));

                // Апгрейды для текущей способности добавляем в базовый пул
                if (Player.Ability is FireBurst)
                {
                    basePerkTypes.AddRange(new[]
                    {
                        PerkType.FireBurstRadiusUp,
                        PerkType.FireBurstDamageUp,
                        PerkType.FireBurstCooldownDown
                    });
                }
                else if (Player.Ability is Barrier)
                {
                    basePerkTypes.AddRange(new[]
                    {
                        PerkType.BarrierDurationUp,
                        PerkType.BarrierCooldownDown
                    });
                }
            }

            // Заполняем оставшиеся слоты случайными перками из пула
            var pool = basePerkTypes.OrderBy(_ => _random.Next()).ToList();
            foreach (var perkType in pool)
            {
                if (OfferedPerks.Count >= PerksToOffer) break;
                OfferedPerks.Add(new Perk(perkType));
            }

            IsChoosingPerk = true;
            _eventBus.Publish(new PerkSelectionEvent(true, OfferedPerks));
        }
        
        private void OnEnemyDeath(IEvent evt)
        {
            if (evt is not EnemyDeathEvent deathEvent) return;

            CreateDrop(deathEvent.Position, deathEvent.ExperienceReward);
            Player.AddExperience(deathEvent.ExperienceReward * Player.ComboExperienceMultiplier);
            Player.AddCombo();
            if (Player.VampirismPercent > 0)
            {
                Player.HealOnKill();
            }
        }

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
