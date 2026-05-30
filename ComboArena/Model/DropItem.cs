using Microsoft.Xna.Framework;

namespace ComboArena.Model
{
    /// <summary>
    /// Тип выпадающего предмета: опыт или здоровье.
    /// </summary>
    public enum DropType
    {
        /// <summary>Сфера опыта - добавляет опыт игроку.</summary>
        Experience,

        /// <summary>Сердце - восстанавливает здоровье игроку.</summary>
        Health
    }

    /// <summary>
    /// Предмет, который выпадает из врагов при смерти.
    /// Можно подобрать, просто пройдя возле него.
    /// Имеет ограниченное время жизни, после чего исчезает.
    /// </summary>
    public class DropItem
    {
        /// <summary>Позиция предмета в игровом мире.</summary>
        public Vector2 Position { get; }

        /// <summary>Тип предмета (опыт или здоровье).</summary>
        public DropType Type { get; }

        /// <summary>Ширина предмета для отрисовки.</summary>
        public static float Width => 60;

        /// <summary>Высота предмета для отрисовки.</summary>
        public static float Height => 60;

        /// <summary>
        /// Активен ли предмет. false означает, что предмет подобран или истекло его время жизни.
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>Количество здоровья или опыта, которое даёт предмет при подборе.</summary>
        private float Value { get; }

        /// <summary>Оставшееся время жизни предмета в секундах (по умолчанию 10 сек).</summary>
        private float _lifeTime = 10f;
        
        /// <summary>
        /// Создаёт выпадающий предмет в указанной позиции.
        /// </summary>
        public DropItem(Vector2 position, DropType type, float value)
        {
            Position = position;
            Type = type;
            Value = value;
        }
        
        /// <summary>
        /// Обновляет состояние предмета каждый кадр.
        /// Уменьшает время жизни; если время вышло - предмет деактивируется.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
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

        /// <summary>
        /// Проверяет, пересекается ли предмет с указанной сущностью (игроком).
        /// </summary>
        public bool CollidesWith(Entity entity)
        {
            return GetBounds().Intersects(entity.GetCollisionBounds());
        }

        /// <summary>
        /// Подбирает предмет: добавляет опыт или здоровье игроку.
        /// После подбора предмет деактивируется.
        /// </summary>
        public void Collect(Player player)
        {
            if (!IsActive) return;

            switch (Type)
            {
                case DropType.Experience:
                    player.AddExperience(Value);
                    break;

                case DropType.Health:
                    // Восстанавливаем здоровье, но не больше максимума
                    player.Health = MathHelper.Clamp(player.Health + Value, 0, player.MaxHealth);
                    break;
            }

            IsActive = false;
        }
        
        /// <summary>
        /// Возвращает прямоугольник предмета для проверки столкновений и отрисовки.
        /// </summary>
        private Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        }
    }
}
