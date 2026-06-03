using ComboArena.Core;
using Microsoft.Xna.Framework;

namespace ComboArena.Model
{
    /// <summary>
    /// Базовый класс для всех игровых объектов (игрок, враги).
    /// Содержит позицию в мире, здоровье, размеры, скорость и методы
    /// для перемещения, получения урона и проверки столкновений.
    /// </summary>
    public abstract class Entity
    {
        public Vector2 Position { get; private set; }

        public float Health { get; set; }

        public float MaxHealth { get; protected set; }

        public float Width { get; }

        public float Height { get; }

        public bool IsAlive => Health > 0;

        protected Vector2 Velocity { get; set; }

        protected float Speed { get; set; }

        protected float CollisionScale { get; set; } = 0.3f;

        protected Entity(float x, float y, float width, float height, float maxHealth, float speed)
        {
            Position = new Vector2(x, y);
            Velocity = Vector2.Zero;
            Width = width;
            Height = height;
            MaxHealth = maxHealth;
            Health = maxHealth; 
            Speed = speed;
        }
        
        public virtual void Update(GameTime gameTime)
        {
            if (!IsAlive) return;

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * delta;
        }

        public void MoveBy(Vector2 offset)
        {
            Position += offset;
        }

        public virtual void TakeDamage(float damage)
        {
            var oldHealth = Health;
            Health -= damage;
            if (Health < 0) Health = 0;
            EventBus.Instance.Publish(new HealthChangedEvent(this, oldHealth, Health));
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        }

        public Rectangle GetCollisionBounds()
        {
            var scaledWidth = Width * CollisionScale;
            var scaledHeight = Height * CollisionScale;
            var offsetX = (Width - scaledWidth) / 2;
            var offsetY = (Height - scaledHeight) / 2;

            return new Rectangle(
                (int)(Position.X + offsetX),
                (int)(Position.Y + offsetY),
                (int)scaledWidth,
                (int)scaledHeight
            );
        }

        public bool CollidesWith(Entity other)
        {
            return GetCollisionBounds().Intersects(other.GetCollisionBounds());
        }
    }
}
