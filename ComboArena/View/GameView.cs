using ComboArena.Controller;
using ComboArena.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ComboArena.View
{
    public class GameView
    {
        private readonly GameController _controller;
        private PlayerView _playerView;
        private Dictionary<EnemyType, EnemyView> _enemyViews;
        private SpriteFont _font;

        public GameView(GameController controller)
        {
            _controller = controller;
            _enemyViews = new Dictionary<EnemyType, EnemyView>();
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font = null)
        {
            _playerView = new PlayerView();
            _playerView.LoadContent(graphicsDevice);
            
            foreach (var type in System.Enum.GetValues(typeof(EnemyType)))
            {
                var view = new EnemyView((EnemyType)type);
                view.LoadContent(graphicsDevice);
                _enemyViews[(EnemyType)type] = view;
            }
            
            _font = font;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_playerView == null || _controller == null || _controller.Player == null)
                return;
            
            _playerView.Draw(spriteBatch, _controller.Player);
            
            foreach (var enemy in _controller.Enemies)
            {
                if (_enemyViews.TryGetValue(enemy.Type, out var view))
                {
                    view.Draw(spriteBatch, enemy);
                }
            }
            
            if (_controller.Player.IsAttacking)
            {
                var attackBounds = _controller.Player.GetAttackBounds();
                _playerView.Draw(spriteBatch, attackBounds, Color.Orange * 0.5f);
            }
        }

        public void DrawUI(SpriteBatch spriteBatch)
        {
            if (_font == null) return;
            
            var healthText = $"Здоровье: {_controller.Player.Health:F0}/{_controller.Player.MaxHealth:F0}";
            spriteBatch.DrawString(_font, healthText, new Vector2(10, 10), Color.White);
        }
    }
}