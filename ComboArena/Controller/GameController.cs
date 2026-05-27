using ComboArena.Core;
using ComboArena.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ComboArena.Controller
{
    public class GameController
    {
        public Player Player { get; }
        public List<Enemy> Enemies { get; }
        public List<DropItem> DropItems { get; }
        public bool IsChoosingPerk { get; private set; }
        public List<Perk> OfferedPerks { get; }
        private List<Entity> _entities { get; }
        private readonly EventBus _eventBus;
        private const int PerksToOffer = 3;
        private readonly Random _random = new();
        private const float SpawnInterval = 1f;
        private const float SpawnZoneWidth = 1200f;
        private const float SpawnZoneHeight = 800f;
        private const float MinSpawnDistance = 300f;
        private const int MaxEnemies = 30;
        private const float MaxEnemyDistance = 2000f; // максимальное расстояние от игрока, после которого враг деспавнится
        private const int MaxSpawnAttempts = 5;
        private float _spawnTimer;
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
            _eventBus.Subscribe("Attack", OnAttack);
            _eventBus.Subscribe("HealthChanged", OnHealthChanged);
        }
        private void StartPerkSelection()
        {
            OfferedPerks.Clear();
            // все возможные типы перков
            var allPerkTypes = new PerkType[]
            {
                PerkType.DamageUp,
                PerkType.AttackSpeedUp,
                PerkType.MaxHealthUp,
                PerkType.SpeedUp,
                PerkType.RangeUp,
                PerkType.Vampirism,
                PerkType.ExperienceBoost
            };
            var usedIndices = new HashSet<int>();
            while (OfferedPerks.Count < PerksToOffer && usedIndices.Count < allPerkTypes.Length)
            {
                var index = _random.Next(allPerkTypes.Length);
                if (!usedIndices.Add(index)) continue;
                OfferedPerks.Add(new Perk(allPerkTypes[index]));
            }
            IsChoosingPerk = true;
        }
        public void SelectPerk(int index)
        {
            if (!IsChoosingPerk || index < 0 || index >= OfferedPerks.Count) return;
            var selectedPerk = OfferedPerks[index];
            Player.ApplyPerk(selectedPerk);
            IsChoosingPerk = false;
            OfferedPerks.Clear();
        }
        public void Update(GameTime gameTime)
        {
            if (IsChoosingPerk) return;
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ProcessInput();
            Player.Update(gameTime);
            UpdateEnemies(gameTime);
            UpdateEnemySpawning(delta);
            ProcessCollisions();
            UpdateDropItems(gameTime);
        }
        private void ProcessInput()
        {
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
            Player.SetMovementDirection(direction);
            Player.SetAttacking(keyboardState.IsKeyDown(Keys.Space));
        }
        private void UpdateEnemies(GameTime gameTime)
        {
            for (var i = Enemies.Count - 1; i >= 0; i--)
            {
                var enemy = Enemies[i];
                enemy.Update(gameTime);
                
                // деспавн врагов, которые слишком далеко от игрока
                var distance = Vector2.Distance(enemy.Position, Player.Position);
                if (distance > MaxEnemyDistance)
                {
                    Enemies.RemoveAt(i);
                    _entities.Remove(enemy);
                    continue;
                }

                if (enemy.IsAlive) continue;
                CreateDrop(enemy.Position, enemy.ExperienceReward);
                Enemies.RemoveAt(i);
                _entities.Remove(enemy);
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
                if (distance < MinSpawnDistance) continue;
                var enemy = CreateRandomEnemy(x, y);
                enemy.SetPlayerTarget(Player);
                Enemies.Add(enemy);
                _entities.Add(enemy);
                _eventBus.Publish(new EnemySpawnedEvent(enemy));
                return;
            }
        }
        private void ProcessCollisions()
        {
            if (!Player.IsAlive) return;
            foreach (var enemy in Enemies)
            {
                var collides = enemy.CollidesWith(Player);
                if (!collides || !enemy.CanAttack()) continue;
                var damage = enemy.Damage;
                Player.TakeDamage(damage);
                enemy.ResetAttackCooldown();
                _eventBus.Publish(new HealthChangedEvent(Player, Player.Health, Player.Health - damage));
            }
            if (Player.IsAttacking)
            {
                foreach (var enemy in Enemies)
                {
                    // проверяем попадание в дугу атаки
                    if (!Player.IsInAttackArc(enemy)) continue;
                    var damage = Player.AttackDamage * Player.GetComboBonusDamage(1f);
                    enemy.TakeDamage(damage);
                    _eventBus.Publish(new AttackEvent(Player, enemy, damage));
                    if (enemy.IsAlive) continue;
                    Player.AddExperience(enemy.ExperienceReward * Player.GetComboBonusExperience(1f));
                    Player.AddCombo();
                    Player.HealOnKill();
                }
                Player.ResetAttackCooldown();
            }
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
            if (_random.NextDouble() < 0.3)
            {
                var healthValue = Player.MaxHealth * 0.15f;
                var healthDrop = new DropItem(position, DropType.Health, healthValue);
                DropItems.Add(healthDrop);
            }
            var expParticles = _random.Next(1, 4);
            for (var i = 0; i < expParticles; i++)
            {
                var offset = new Vector2((float)(_random.NextDouble() * 40 - 20), (float)(_random.NextDouble() * 40 - 20));
                var expValue = experienceReward / expParticles;
                var expDrop = new DropItem(position + offset, DropType.Experience, expValue);
                DropItems.Add(expDrop);
            }
        }
        private void OnAttack(IEvent evt)
        {
        }
        private void OnHealthChanged(IEvent evt)
        {
        }
        private void OnPlayerLevelUp(int newLevel)
        {
            if (newLevel % 3 == 0)
            {
                StartPerkSelection();
            }
        }
    }
}
