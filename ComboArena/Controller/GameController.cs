using ComboArena.Core;
using ComboArena.Model;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ComboArena.Controller
{
    public class GameController
    {
        public Player Player { get; private set; }
        public List<Enemy> Enemies { get; private set; }
        public List<Entity> Entities { get; private set; }
        
        private readonly EventBus _eventBus;
        private float _enemySpawnTimer = 0f;
        private const float EnemySpawnInterval = 1f;
        private const float SpawnZoneWidth = 1200f;
        private const float SpawnZoneHeight = 800f;
        private const float DespawnZoneWidth = 1600f;
        private const float DespawnZoneHeight = 1200f;
        private const float VisibleZoneWidth = 960f;
        private const float VisibleZoneHeight = 540f;
        private const int MaxEnemies = 15;
        private const float MinSpawnDistance = 300f;
        private Random _random = new Random();

        public GameController()
        {
            _eventBus = EventBus.Instance;
            Enemies = new List<Enemy>();
            Entities = new List<Entity>();
            
            Player = new Player(0, 0);
            Entities.Add(Player);
            
            _eventBus.Subscribe("Attack", OnAttack);
            _eventBus.Subscribe("HealthChanged", OnHealthChanged);
        }

        public void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            Player.Update(gameTime);
            
            foreach (var enemy in Enemies)
            {
                enemy.SetPlayerTarget(Player);
                enemy.Update(gameTime);

                if (!enemy.CollidesWith(Player) || !enemy.CanAttack()) continue;
                Player.TakeDamage(enemy.Damage);
                enemy.ResetAttackCooldown();
                    
                _eventBus.Publish(new AttackEvent(enemy, Player, enemy.Damage));
            }
            
            if (Player.IsAttacking)
            {
                foreach (var enemy in Enemies)
                {
                    if (!Player.IsInAttackArc(enemy)) continue;
                    enemy.TakeDamage(Player.AttackDamage);
                    _eventBus.Publish(new AttackEvent(Player, enemy, Player.AttackDamage));
                }
            }
            
            for (var i = Enemies.Count - 1; i >= 0; i--)
            {
                var enemy = Enemies[i];
                if (!enemy.IsAlive)
                {
                    Entities.Remove(enemy);
                    Enemies.RemoveAt(i);
                    continue;
                }
                
                var diff = enemy.Position - Player.Position;
                var halfWidth = DespawnZoneWidth / 2;
                var halfHeight = DespawnZoneHeight / 2;
                
                var visibleHalfWidth = VisibleZoneWidth / 2;
                var visibleHalfHeight = VisibleZoneHeight / 2;
                var isOffScreen = Math.Abs(diff.X) > visibleHalfWidth || Math.Abs(diff.Y) > visibleHalfHeight;
                
                if (isOffScreen)
                {
                    enemy.TimeOffScreen += delta;
                    
                    const float maxTimeOffScreen = 30f;
                    if (enemy.TimeOffScreen >= maxTimeOffScreen)
                    {
                        Entities.Remove(enemy);
                        Enemies.RemoveAt(i);
                        continue;
                    }
                }
                else
                {
                    enemy.TimeOffScreen = 0f;
                }
                
                if (Math.Abs(diff.X) > halfWidth || Math.Abs(diff.Y) > halfHeight)
                {
                    Entities.Remove(enemy);
                    Enemies.RemoveAt(i);
                }
            }
            
            _enemySpawnTimer -= delta;
            if (_enemySpawnTimer <= 0)
            {
                SpawnEnemy();
                _enemySpawnTimer = EnemySpawnInterval;
            }
        }

        private void SpawnEnemy()
        {
            if (Enemies.Count >= MaxEnemies)
                return;
            
            var playerCenter = Player.Position + new Vector2(Player.Width / 2, Player.Height / 2);
            var spawnHalfWidth = SpawnZoneWidth / 2;
            var spawnHalfHeight = SpawnZoneHeight / 2;
            
            const int maxAttempts = 5;
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var x = playerCenter.X + ((float)_random.NextDouble() * SpawnZoneWidth - spawnHalfWidth);
                var y = playerCenter.Y + ((float)_random.NextDouble() * SpawnZoneHeight - spawnHalfHeight);
                
                var dx = x - playerCenter.X;
                var dy = y - playerCenter.Y;
                var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                
                if (distance >= MinSpawnDistance)
                {
                    var enemyType = (EnemyType)_random.Next(0, 3);
                    var enemy = new Enemy(x, y, enemyType);
                    Enemies.Add(enemy);
                    Entities.Add(enemy);
                    
                    _eventBus.Publish(new EnemySpawnedEvent(enemy));
                    return;
                }
            }
            
            var fallbackX = playerCenter.X + ((float)_random.NextDouble() * SpawnZoneWidth - spawnHalfWidth);
            var fallbackY = playerCenter.Y + ((float)_random.NextDouble() * SpawnZoneHeight - spawnHalfHeight);
            var fallbackEnemyType = (EnemyType)_random.Next(0, 3);
            var fallbackEnemy = new Enemy(fallbackX, fallbackY, fallbackEnemyType);
            Enemies.Add(fallbackEnemy);
            Entities.Add(fallbackEnemy);
            
            _eventBus.Publish(new EnemySpawnedEvent(fallbackEnemy));
        }

        private void OnAttack(IEvent evt)
        {
            if (evt is AttackEvent attackEvent)
            {
            }
        }

        private void OnHealthChanged(IEvent evt)
        {
            if (evt is not HealthChangedEvent healthEvent) return;
        }
    }
}