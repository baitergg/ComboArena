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
        /// <summary>Позиция объекта в игровом мире (в пикселях).</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Текущее количество здоровья объекта.</summary>
        public float Health { get; set; }

        /// <summary>Максимальное количество здоровья объекта.</summary>
        public float MaxHealth { get; protected set; }

        /// <summary>Ширина объекта (используется для коллизий и отрисовки).</summary>
        public float Width { get; }

        /// <summary>Высота объекта (используется для коллизий и отрисовки).</summary>
        public float Height { get; }

        /// <summary>Возвращает true, если объект жив.</summary>
        public bool IsAlive => Health > 0;

        /// <summary>Текущий вектор скорости движения объекта.</summary>
        protected Vector2 Velocity { get; set; }

        /// <summary>Максимальная скорость передвижения объекта (пикселей/сек).</summary>
        protected float Speed { get; set; }

        /// <summary>
        /// Масштаб хитбокса относительно визуального размера (0..1).
        /// 1.0 = весь объект, 0.3 = маленькая зона в центре.
        /// </summary>
        protected float CollisionScale { get; set; } = 0.3f;

        /// <summary>
        /// Инициализирует базовую сущность с заданными параметрами.
        /// </summary>
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
        
        /// <summary>
        /// Обновляет состояние объекта каждый кадр.
        /// Перемещает объект в соответствии с текущей скоростью.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
        public virtual void Update(GameTime gameTime)
        {
            if (!IsAlive) return;

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * delta;
        }

        /// <summary>
        /// Смещает позицию объекта на заданный вектор.
        /// Используется для отталкивания от щита.
        /// </summary>
        /// <param name="offset">Вектор смещения.</param>
        public void MoveBy(Vector2 offset)
        {
            Position += offset;
        }

        /// <summary>
        /// Наносит урон объекту.
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            var oldHealth = Health;
            Health -= damage;
            if (Health < 0) Health = 0;
            EventBus.Instance.Publish(new HealthChangedEvent(this, oldHealth, Health));
        }

        /// <summary>
        /// Возвращает прямоугольник для отрисовки.
        /// </summary>
        public Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        }

        /// <summary>
        /// Возвращает прямоугольник для проверки столкновений.
        /// Размер определяется параметром CollisionScale.
        /// </summary>
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

        /// <summary>
        /// Проверяет, пересекается ли хитбокс этого объекта с хитбоксом другого.
        /// </summary>
        /// <param name="other">Другая сущность для проверки.</param>
        /// <returns>true, если объекты пересекаются.</returns>
        public bool CollidesWith(Entity other)
        {
            return GetCollisionBounds().Intersects(other.GetCollisionBounds());
        }
    }
}
