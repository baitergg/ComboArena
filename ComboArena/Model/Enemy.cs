using Microsoft.Xna.Framework;
using System;

namespace ComboArena.Model
{
    public enum EnemyType
    {
        Red,
        Blue,
        Yellow
    }

    public class Enemy : Entity
    {
        public EnemyType Type { get; }
        public float Damage { get; set; }
        public float AttackCooldown { get; set; }
        public float DetectionRange { get; set; }
        public float TimeOffScreen { get; set; }
        private float _attackTimer = 0f;
        private Random _random = new Random();
        private Vector2 _targetDirection;
        private float _directionChangeTimer = 0f;
        private Player _playerTarget;

        public Enemy(float x, float y, EnemyType type) 
            : base(x, y, width: 25, height: 25, maxHealth: GetMaxHealth(type), speed: GetSpeed(type))
        {
            Type = type;
            Damage = GetDamage(type);
            AttackCooldown = GetAttackCooldown(type);
            DetectionRange = GetDetectionRange(type);
            
            _targetDirection = new Vector2((float)_random.NextDouble() * 2 - 1, (float)_random.NextDouble() * 2 - 1);
            if (_targetDirection.LengthSquared() > 0)
                _targetDirection.Normalize();
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
                
                if (distance <= DetectionRange)
                {
                    if (toPlayer.LengthSquared() > 0)
                    {
                        _targetDirection = toPlayer;
                        _targetDirection.Normalize();
                    }
                }
                else
                {
                    _directionChangeTimer -= delta;
                    if (_directionChangeTimer <= 0)
                    {
                        _targetDirection = new Vector2((float)_random.NextDouble() * 2 - 1,
                            (float)_random.NextDouble() * 2 - 1);
                        if (_targetDirection.LengthSquared() > 0)
                            _targetDirection.Normalize();
                        _directionChangeTimer = _random.Next(1, 3);
                    }
                }
            }
            
            Velocity = _targetDirection * Speed;
            
            if (_attackTimer > 0)
                _attackTimer -= delta;
        }

        public bool CanAttack()
        {
            return _attackTimer <= 0;
        }

        public void ResetAttackCooldown()
        {
            _attackTimer = AttackCooldown;
        }

        private static float GetMaxHealth(EnemyType type)
        {
            return type switch
            {
                EnemyType.Red => 20,
                EnemyType.Blue => 40,
                EnemyType.Yellow => 60,
                _ => 30
            };
        }

        private static float GetSpeed(EnemyType type)
        {
            return type switch
            {
                EnemyType.Red => 120,
                EnemyType.Blue => 90,
                EnemyType.Yellow => 60,
                _ => 80
            };
        }

        private static float GetDamage(EnemyType type)
        {
            return type switch
            {
                EnemyType.Red => 3,
                EnemyType.Blue => 6,
                EnemyType.Yellow => 10,
                _ => 5
            };
        }

        private static float GetAttackCooldown(EnemyType type)
        {
            return type switch
            {
                EnemyType.Red => 0.8f,
                EnemyType.Blue => 1.2f,
                EnemyType.Yellow => 1.8f,
                _ => 1f
            };
        }

        private static float GetDetectionRange(EnemyType type)
        {
            return type switch
            {
                EnemyType.Red => 150,
                EnemyType.Blue => 200,
                EnemyType.Yellow => 250,
                _ => 180
            };
        }
    }
}