using ComboArena.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ComboArena.View
{
    /// <summary>
    /// Базовый класс для отрисовки сущностей (Entity) на экране.
    /// Загружает текстуру и предоставляет методы для рисования
    /// с использованием SpriteBatch.
    /// </summary>
    public class EntityView
    {
        /// <summary>Текстура, используемая для отрисовки сущности.</summary>
        protected Texture2D Texture;

        /// <summary>Белая текстура 1x1 для рисования линий и прямоугольников.</summary>
        private Texture2D _pixelTexture;

        /// <summary>Имя текстуры для загрузки через ContentManager.</summary>
        private readonly string _textureName;
        
        /// <summary>
        /// Создаёт объект отрисовки сущности.
        /// </summary>
        public EntityView(string textureName = null)
        {
            _textureName = textureName;
        }

        /// <summary>
        /// Загружает контент: текстуру сущности и пиксельную текстуру для отладки/UI.
        /// </summary>
        public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content = null)
        {
            if (!string.IsNullOrEmpty(_textureName) && content != null)
            {
                Texture = content.Load<Texture2D>(_textureName);
            }
            else
            {
                Texture = new Texture2D(graphicsDevice, 1, 1);
                Texture.SetData(new[] { Color.White });
            }

            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        /// <summary>
        /// Рисует сущность в её границах.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch, Entity entity)
        {
            if (Texture == null) return;

            var bounds = entity.GetBounds();
            spriteBatch.Draw(Texture, bounds, Color.White);
        }

        /// <summary>
        /// Рисует залитый прямоугольник с использованием пиксельной текстуры.
        /// </summary>
        protected void DrawPixel(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            if (_pixelTexture == null) return;
            spriteBatch.Draw(_pixelTexture, bounds, color);
        }
    }

    /// <summary>
    /// Отрисовка игрока. Использует текстуру "Player"
    /// и дополнительно рисует полоску здоровья над персонажем.
    /// </summary>
    public class PlayerView : EntityView
    {
        /// <summary>
        /// Создаёт отображение игрока с текстурой "Player".
        /// </summary>
        public PlayerView() : base("Player")
        {
        }

        /// <summary>
        /// Рисует игрока и его полоску здоровья.
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch, Entity entity)
        {
            if (Texture == null || entity == null) return;

            base.Draw(spriteBatch, entity);

            if (entity is Player playerWithHealth)
            {
                DrawHealthBar(spriteBatch, playerWithHealth);
            }
        }

        /// <summary>
        /// Рисует полоску здоровья над игроком.
        /// Цвет меняется в зависимости от процента здоровья:
        /// зелёный (>50%), оранжевый (>25%), красный (<=25%).
        /// </summary>
        private void DrawHealthBar(SpriteBatch spriteBatch, Player player)
        {
            const int barWidth = 40;
            const int barHeight = 6;

            var x = (int)player.Position.X + (int)player.Width / 2 - barWidth / 2;
            var y = (int)player.Position.Y - 12;

            // Фон полоски
            var backgroundRect = new Rectangle(x, y, barWidth, barHeight);
            DrawPixel(spriteBatch, backgroundRect, new Color(60, 60, 60, 200));

            // Заполнение
            var healthPercent = player.Health / player.MaxHealth;
            var healthWidth = (int)(barWidth * healthPercent);

            if (healthWidth > 0)
            {
                var healthRect = new Rectangle(x, y, healthWidth, barHeight);
                var healthColor = healthPercent > 0.5f ? Color.LimeGreen :
                                 healthPercent > 0.25f ? Color.Orange : Color.Red;
                DrawPixel(spriteBatch, healthRect, healthColor);
            }

            // Рамка
            var borderRect = new Rectangle(x - 1, y - 1, barWidth + 2, barHeight + 2);
            DrawPixel(spriteBatch, borderRect, Color.Black * 0.5f);
        }
    }

    /// <summary>
    /// Отрисовка врага. Использует текстуру в зависимости от типа врага
    /// (Red_enemy, Blue_enemy, Yellow_enemy) и рисует полоску здоровья.
    /// </summary>
    public class EnemyView : EntityView
    {
        /// <summary>Тип врага для выбора текстуры.</summary>
        private readonly EnemyType _type;

        /// <summary>
        /// Создаёт отображение врага с текстурой, соответствующей его типу.
        /// </summary>
        public EnemyView(EnemyType type) : base(GetTextureNameForType(type))
        {
            _type = type;
        }

        /// <summary>
        /// Возвращает имя текстуры для указанного типа врага.
        /// </summary>
        private static string GetTextureNameForType(EnemyType type)
        {
            return type switch
            {
                EnemyType.Red => "Red_enemy",
                EnemyType.Blue => "Blue_enemy",
                EnemyType.Yellow => "Yellow_enemy",
                _ => null
            };
        }

        /// <summary>
        /// Рисует врага и его полоску здоровья.
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch, Entity entity)
        {
            if (Texture == null || entity == null) return;

            base.Draw(spriteBatch, entity);

            if (entity is Enemy enemy)
            {
                DrawHealthBar(spriteBatch, enemy);
            }
        }

        /// <summary>
        /// Рисует полоску здоровья над врагом.
        /// </summary>
        private void DrawHealthBar(SpriteBatch spriteBatch, Enemy enemy)
        {
            const int barWidth = 30;
            const int barHeight = 5;

            var x = (int)enemy.Position.X + (int)enemy.Width / 2 - barWidth / 2;
            var y = (int)enemy.Position.Y - 10;

            // Фон полоски
            var backgroundRect = new Rectangle(x, y, barWidth, barHeight);
            DrawPixel(spriteBatch, backgroundRect, new Color(40, 40, 40, 200));

            // Заполнение
            var healthPercent = enemy.Health / enemy.MaxHealth;
            var healthWidth = (int)(barWidth * healthPercent);

            if (healthWidth > 0)
            {
                var healthRect = new Rectangle(x, y, healthWidth, barHeight);
                var healthColor = healthPercent > 0.5f ? Color.LimeGreen :
                                 healthPercent > 0.25f ? Color.Orange : Color.Red;
                DrawPixel(spriteBatch, healthRect, healthColor);
            }

            // Рамка
            var borderRect = new Rectangle(x - 1, y - 1, barWidth + 2, barHeight + 2);
            DrawPixel(spriteBatch, borderRect, Color.Black * 0.5f);
        }
    }
}
