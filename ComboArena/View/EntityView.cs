using ComboArena.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ComboArena.View
{
    public class EntityView
    {
        protected Texture2D Texture;
        private Texture2D _pixelTexture;
        private readonly string _textureName;
        
        public EntityView(string textureName = null)
        {
            _textureName = textureName;
        }
        
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
        
        public virtual void Draw(SpriteBatch spriteBatch, Entity entity)
        {
            if (Texture == null) return;
            
            var bounds = entity.GetBounds();
            spriteBatch.Draw(Texture, bounds, Color.White);
        }
        
        public void Draw(SpriteBatch spriteBatch, Rectangle bounds, Color? color = null)
        {
            if (Texture == null) return;
            
            spriteBatch.Draw(Texture, bounds, color ?? Color.White);
        }
        
        protected void DrawPixel(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            if (_pixelTexture == null) return;
            spriteBatch.Draw(_pixelTexture, bounds, color);
        }
    }
    
    public class PlayerView : EntityView
    {
        public PlayerView() : base("Player")
        {
        }
        
        public override void Draw(SpriteBatch spriteBatch, Entity entity)
        {
            if (Texture == null || entity == null) return;
            
            base.Draw(spriteBatch, entity);
            
            if (entity is Player playerWithHealth)
            {
                DrawHealthBar(spriteBatch, playerWithHealth);
            }
        }
        
        private void DrawHealthBar(SpriteBatch spriteBatch, Player player)
        {
            const int barWidth = 40;
            const int barHeight = 6;
            
            var x = (int)player.Position.X + (int)player.Width / 2 - barWidth / 2;
            var y = (int)player.Position.Y - 12;
            
            var backgroundRect = new Rectangle(x, y, barWidth, barHeight);
            DrawPixel(spriteBatch, backgroundRect, new Color(60, 60, 60, 200));
            
            var healthPercent = player.Health / player.MaxHealth;
            var healthWidth = (int)(barWidth * healthPercent);
            
            if (healthWidth > 0)
            {
                var healthRect = new Rectangle(x, y, healthWidth, barHeight);
                var healthColor = healthPercent > 0.5f ? Color.LimeGreen :
                                 healthPercent > 0.25f ? Color.Orange : Color.Red;
                DrawPixel(spriteBatch, healthRect, healthColor);
            }
            
            var borderRect = new Rectangle(x - 1, y - 1, barWidth + 2, barHeight + 2);
            DrawPixel(spriteBatch, borderRect, Color.Black * 0.5f);
        }
    }
    
    public class EnemyView : EntityView
    {
        private readonly EnemyType _type;
        
        public EnemyView(EnemyType type) : base(GetTextureNameForType(type))
        {
            _type = type;
        }
        
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
        
        public override void Draw(SpriteBatch spriteBatch, Entity entity)
        {
            if (Texture == null || entity == null) return;
            
            base.Draw(spriteBatch, entity);
            
            if (entity is Enemy enemy)
            {
                DrawHealthBar(spriteBatch, enemy);
            }
        }
        
        private void DrawHealthBar(SpriteBatch spriteBatch, Enemy enemy)
        {
            const int barWidth = 30;
            const int barHeight = 5;
            
            var x = (int)enemy.Position.X + (int)enemy.Width / 2 - barWidth / 2;
            var y = (int)enemy.Position.Y - 10;
            
            var backgroundRect = new Rectangle(x, y, barWidth, barHeight);
            DrawPixel(spriteBatch, backgroundRect, new Color(40, 40, 40, 200));
            
            var healthPercent = enemy.Health / enemy.MaxHealth;
            var healthWidth = (int)(barWidth * healthPercent);
            
            if (healthWidth > 0)
            {
                var healthRect = new Rectangle(x, y, healthWidth, barHeight);
                var healthColor = healthPercent > 0.5f ? Color.LimeGreen :
                                 healthPercent > 0.25f ? Color.Orange : Color.Red;
                DrawPixel(spriteBatch, healthRect, healthColor);
            }
            
            var borderRect = new Rectangle(x - 1, y - 1, barWidth + 2, barHeight + 2);
            DrawPixel(spriteBatch, borderRect, Color.Black * 0.5f);
        }
    }
}
