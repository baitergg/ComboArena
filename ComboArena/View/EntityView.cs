using ComboArena.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComboArena.View
{
    public class EntityView
    {
        protected Texture2D _texture;
        private Color _color;

        public EntityView(Color color)
        {
            _color = color;
        }

        public virtual void LoadContent(GraphicsDevice graphicsDevice)
        {
            _texture = new Texture2D(graphicsDevice, 1, 1);
            _texture.SetData(new[] { Color.White });
        }

        public virtual void Draw(SpriteBatch spriteBatch, Entity entity)
        {
            if (_texture == null) return;
            
            var bounds = entity.GetBounds();
            spriteBatch.Draw(_texture, bounds, _color);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle bounds, Color? color = null)
        {
            if (_texture == null) return;
            spriteBatch.Draw(_texture, bounds, color ?? _color);
        }
    }

    public class PlayerView : EntityView
    {
        public PlayerView() : base(Color.Black)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, Entity entity)
        {
            base.Draw(spriteBatch, entity);

            if (entity is Player player)
            {
                DrawHealthBar(spriteBatch, player);
            }
        }

        private void DrawHealthBar(SpriteBatch spriteBatch, Player player)
        {
            const int barWidth = 40;
            const int barHeight = 5;
            var x = (int)player.Position.X + (int)player.Width / 2 - barWidth / 2;
            var y = (int)player.Position.Y - 10;
            
            var backgroundRect = new Rectangle(x, y, barWidth, barHeight);
            spriteBatch.Draw(_texture, backgroundRect, Color.Gray);
            
            var healthPercent = player.Health / player.MaxHealth;
            var healthWidth = (int)(barWidth * healthPercent);
            if (healthWidth <= 0) return;
            var healthRect = new Rectangle(x, y, healthWidth, barHeight);
            spriteBatch.Draw(_texture, healthRect, Color.Green);
        }
    }   

    public class EnemyView : EntityView
    {
        private readonly EnemyType _type;

        public EnemyView(EnemyType type) : base(GetColorForType(type))
        {
            _type = type;
        }

        private static Color GetColorForType(EnemyType type)
        {
            return type switch
            {
                EnemyType.Red => Color.Red,
                EnemyType.Blue => Color.Blue,
                EnemyType.Yellow => Color.Yellow,
                _ => Color.White
            };
        }

        public override void Draw(SpriteBatch spriteBatch, Entity entity)
        {
            base.Draw(spriteBatch, entity);

            if (entity is Enemy enemy)
            {
                DrawHealthBar(spriteBatch, enemy);
            }
        }

        private void DrawHealthBar(SpriteBatch spriteBatch, Enemy enemy)
        {
            const int barWidth = 30;
            const int barHeight = 4;
            var x = (int)enemy.Position.X + (int)enemy.Width / 2 - barWidth / 2;
            var y = (int)enemy.Position.Y - 8;
            
            var backgroundRect = new Rectangle(x, y, barWidth, barHeight);
            spriteBatch.Draw(_texture, backgroundRect, Color.DarkGray);
            
            var healthPercent = enemy.Health / enemy.MaxHealth;
            var healthWidth = (int)(barWidth * healthPercent);
            if (healthWidth <= 0) return;
            var healthRect = new Rectangle(x, y, healthWidth, barHeight);
            spriteBatch.Draw(_texture, healthRect, Color.Lime);
        }
    }
}