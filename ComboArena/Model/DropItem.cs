using Microsoft.Xna.Framework;

namespace ComboArena.Model
{
    public enum DropType
    {
        Experience,

        Health
    }

    /// <summary>
    /// Предмет, который выпадает из врагов при смерти.
    /// Можно подобрать, просто пройдя возле него.
    /// Имеет ограниченное время жизни, после чего исчезает.
    /// </summary>
    public class DropItem
    {
        public Vector2 Position { get; }

        public DropType Type { get; }

        public static float Width => 60;

        public static float Height => 60;

        public bool IsActive { get; private set; } = true;

        /// <summary>Количество здоровья или опыта, которое даёт предмет при подборе.</summary>
        private float Value { get; }

        private float _lifeTime = 10f;
        
        public DropItem(Vector2 position, DropType type, float value)
        {
            Position = position;
            Type = type;
            Value = value;
        }
        
        public void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _lifeTime -= delta;

            if (_lifeTime <= 0)
            {
                IsActive = false;
            }
        }

        public bool CollidesWith(Entity entity)
        {
            return GetBounds().Intersects(entity.GetCollisionBounds());
        }

        public void Collect(Player player)
        {
            if (!IsActive) return;

            switch (Type)
            {
                case DropType.Experience:
                    player.AddExperience(Value);
                    break;

                case DropType.Health:
                    player.Health = MathHelper.Clamp(player.Health + Value, 0, player.MaxHealth);
                    break;
            }

            IsActive = false;
        }
        
        private Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        }
    }
}
