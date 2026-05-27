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
    public abstract class Enemy : Entity
    {
        public abstract EnemyType Type { get; }
        public float Damage { get; }
        public float ExperienceReward { get; }
        private float AttackCooldown { get; }
        private float DetectionRange { get; }
        private float _attackTimer;
        private readonly Random _random = new();
        private Vector2 _targetDirection;
        private float _directionChangeTimer;
        private Player _playerTarget;
        protected Enemy(float x, float y, float width, float height, float maxHealth, float speed,
            float damage, float attackCooldown, float detectionRange, float experienceReward)
            : base(x, y, width, height, maxHealth, speed)
        {
            Damage = damage;
            AttackCooldown = attackCooldown;
            DetectionRange = detectionRange;
            ExperienceReward = experienceReward;
            _targetDirection = new Vector2((float)_random.NextDouble() * 2 - 1, (float)_random.NextDouble() * 2 - 1);
            if (_targetDirection.LengthSquared() > 0)
                _targetDirection.Normalize();
            CollisionScale = 0.35f;
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
    }
}
