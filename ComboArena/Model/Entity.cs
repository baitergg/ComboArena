using Microsoft.Xna.Framework;

namespace ComboArena.Model
{
    public abstract class Entity
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Speed { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool IsAlive => Health > 0;

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

        public virtual void TakeDamage(float damage)
        {
            var oldHealth = Health;
            Health -= damage;
            if (Health < 0) Health = 0;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        }

        public bool CollidesWith(Entity other)
        {
            return GetBounds().Intersects(other.GetBounds());
        }
    }
}