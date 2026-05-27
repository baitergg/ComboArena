using ComboArena.Controller;
using ComboArena.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ComboArena.View
{
    public class GameView
    {
        private readonly GameController _controller;
        private PlayerView _playerView;
        private Dictionary<EnemyType, EnemyView> _enemyViews;
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        private Texture2D _expTexture;
        private Texture2D _heartTexture;
        private Texture2D _attackTextureRight;
        private Texture2D _attackTextureLeft;
        
        public GameView(GameController controller)
        {
            _controller = controller;
            _enemyViews = new Dictionary<EnemyType, EnemyView>();
        }
        
        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font = null, ContentManager content = null)
        {
            _playerView = new PlayerView();
            _playerView.LoadContent(graphicsDevice, content);
            
            foreach (var type in System.Enum.GetValues(typeof(EnemyType)))
            {
                var view = new EnemyView((EnemyType)type);
                view.LoadContent(graphicsDevice, content);
                _enemyViews[(EnemyType)type] = view;
            }
            
            _font = font;
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _expTexture = content.Load<Texture2D>("exp");
            _heartTexture = content.Load<Texture2D>("heart");
            _attackTextureRight = content.Load<Texture2D>("attack_right");
            _attackTextureLeft = content.Load<Texture2D>("attack_left");
         
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
                var attackTexture = _controller.Player.FacingDirection.X > 0 ? _attackTextureRight : _attackTextureLeft;
                spriteBatch.Draw(attackTexture, attackBounds, Color.Orange * 0.7f);
            }
            
            DrawDrops(spriteBatch);
        }
        
        public void DrawDrops(SpriteBatch spriteBatch)
        {
            var time = (float)System.DateTime.Now.TimeOfDay.TotalSeconds;
            
            foreach (var drop in _controller.DropItems)
            {
                var rect = new Rectangle((int)drop.Position.X, (int)drop.Position.Y, (int)DropItem.Width, (int)DropItem.Height);
                
                var pulse = (float)(System.Math.Sin(time * 5 + drop.Position.X * 0.1f) * 0.2 + 0.8);
                var pulseColor = Color.White * pulse;
                
                Texture2D texture = drop.Type == DropType.Experience ? _expTexture : _heartTexture;
                spriteBatch.Draw(texture, rect, pulseColor);
            }
        }
        
        public void DrawUI(SpriteBatch spriteBatch)
        {
            if (_font == null) return;
            
            if (_controller.IsChoosingPerk)
            {
                DrawPerkSelection(spriteBatch);
                return;
            }
            
            var player = _controller.Player;
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            
            var uiBackground = new Rectangle(5, 5, 260, player.ComboCount > 0 ? 220 : 160);
            spriteBatch.Draw(_pixelTexture, uiBackground, new Color(0, 0, 0, 150));
            
            void DrawTextWithShadow(string text, Vector2 position, Color color)
            {
               
                spriteBatch.DrawString(_font, text, position + new Vector2(1, 1), Color.Black * 0.5f);
                spriteBatch.DrawString(_font, text, position, color);
            }
            
            void DrawProgressBar(Vector2 position, int width, int height, float progress, Color color, string label = "")
            {
                var backgroundRect = new Rectangle((int)position.X, (int)position.Y, width, height);
                spriteBatch.Draw(_pixelTexture, backgroundRect, new Color(40, 40, 40, 200));
                
                var fillWidth = (int)(width * progress);
                if (fillWidth > 0)
                {
                    var fillRect = new Rectangle((int)position.X, (int)position.Y, fillWidth, height);
                    spriteBatch.Draw(_pixelTexture, fillRect, color);
                }
                
                var borderRect = new Rectangle((int)position.X - 1, (int)position.Y - 1, width + 2, height + 2);
                spriteBatch.Draw(_pixelTexture, borderRect, Color.Black * 0.5f);
                
                if (!string.IsNullOrEmpty(label))
                {
                    var labelText = $"{label}: {(progress * 100):F0}%";
                    DrawTextWithShadow(labelText, position + new Vector2(width + 10, -2), Color.White * 0.8f);
                }
            }
            
            DrawTextWithShadow($"HP: {player.Health:F0}/{player.MaxHealth:F0}", new Vector2(15, 15), Color.Red);
            DrawProgressBar(new Vector2(15, 35), 200, 10, player.Health / player.MaxHealth, Color.Red, "Health");
            
            DrawTextWithShadow($"Level: {player.Level}", new Vector2(15, 55), Color.Green);
            
            DrawTextWithShadow($"XP: {player.Experience:F0}/{player.ExperienceToNextLevel:F0}", new Vector2(15, 75), Color.Blue);
            DrawProgressBar(new Vector2(15, 95), 200, 10, player.Experience / player.ExperienceToNextLevel, Color.Blue, "XP");
            
            if (player.ComboCount > 0)
            {
                var comboY = 120;
                var comboText = $"COMBO: {player.ComboCount}x";
                var comboColor = Color.Lerp(Color.Yellow, Color.Red, player.ComboCount / 50f);
                DrawTextWithShadow(comboText, new Vector2(15, comboY), comboColor);
                
                var bonusText = $"Damage: +{((player.ComboDamageMultiplier - 1) * 100):F0}%  XP: +{((player.ComboExperienceMultiplier - 1) * 100):F0}%";
                DrawTextWithShadow(bonusText, new Vector2(15, comboY + 20), Color.LightGreen);
                
                var timerBarWidth = 200;
                var timerBarHeight = 8;
                var timerBarPosition = new Vector2(15, comboY + 45);
                var timerFillWidth = (int)(timerBarWidth * (player.ComboTimer / Player.ComboTimeout));
                
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle((int)timerBarPosition.X, (int)timerBarPosition.Y, timerBarWidth, timerBarHeight),
                    new Color(60, 60, 60, 200));
                
                var timerColor = Color.Lerp(Color.Cyan, Color.Magenta, player.ComboTimer / Player.ComboTimeout);
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle((int)timerBarPosition.X, (int)timerBarPosition.Y, timerFillWidth, timerBarHeight),
                    timerColor);
                
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle((int)timerBarPosition.X - 1, (int)timerBarPosition.Y - 1,
                        timerBarWidth + 2, timerBarHeight + 2),
                    Color.Black * 0.5f);
                
                DrawTextWithShadow("Combo Timer", timerBarPosition + new Vector2(timerBarWidth + 10, -2), Color.White * 0.8f);
            }
            
            var perkY = player.ComboCount > 0 ? 180 : 120;
            DrawTextWithShadow("Active Perks:", new Vector2(15, perkY), Color.Gold);
            
            perkY += 20;
            foreach (var perk in player.ActivePerks.Take(5))
            {
                var perkText = $"- {perk.Name}";
                DrawTextWithShadow(perkText, new Vector2(20, perkY), Color.Yellow);
                perkY += 18;
            }
            
            var enemyCount = _controller.Enemies.Count;
            var enemyText = $"Enemies: {enemyCount}";
            var enemyTextSize = _font.MeasureString(enemyText);
            var enemyTextPosition = new Vector2(viewport.Width - enemyTextSize.X - 20, 15);
            DrawTextWithShadow(enemyText, enemyTextPosition, Color.Orange);
        }
        
        public void DrawPerkSelection(SpriteBatch spriteBatch)
        {
            if (!_controller.IsChoosingPerk) return;
            
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            var screenWidth = viewport.Width;
            var screenHeight = viewport.Height;
            
            var background = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(_pixelTexture, background, new Color(0, 0, 0, 180));
            
            var title = "CHOOSE A PERK";
            var titleSize = _font.MeasureString(title);
            var titlePos = new Vector2((screenWidth - titleSize.X) / 2, screenHeight * 0.1f);
            DrawTextWithShadow(spriteBatch, title, titlePos, Color.Gold);
            
            var cardWidth = 320;
            var cardHeight = 240;
            var spacing = 40;
            var totalWidth = _controller.OfferedPerks.Count * cardWidth + (_controller.OfferedPerks.Count - 1) * spacing;
            var startX = (screenWidth - totalWidth) / 2;
            var y = screenHeight * 0.3f;
            
            for (var i = 0; i < _controller.OfferedPerks.Count; i++)
            {
                var perk = _controller.OfferedPerks[i];
                var x = startX + i * (cardWidth + spacing);
                var cardRect = new Rectangle((int)x, (int)y, cardWidth, cardHeight);
                
                var cardColor = new Color(40, 40, 60, 240);
                var highlightColor = new Color(80, 80, 100, 255);
                
                spriteBatch.Draw(_pixelTexture, cardRect, cardColor);
                
                var highlightRect = new Rectangle(cardRect.X, cardRect.Y, cardRect.Width, 10);
                spriteBatch.Draw(_pixelTexture, highlightRect, highlightColor);
                
                var borderRect = new Rectangle(cardRect.X - 2, cardRect.Y - 2, cardRect.Width + 4, cardRect.Height + 4);
                spriteBatch.Draw(_pixelTexture, borderRect, Color.Gold * 0.5f);
                
                var titlePosCard = new Vector2(x + 20, y + 20);
                DrawTextWithShadow(spriteBatch, perk.Name, titlePosCard, Color.Yellow);
                
                var descPos = new Vector2(x + 20, y + 60);
                var wrappedDescription = WrapText(perk.Description, cardWidth - 40);
                DrawTextWithShadow(spriteBatch, wrappedDescription, descPos, Color.White);
                
                var keyText = $"[{i + 1}]";
                var keySize = _font.MeasureString(keyText);
                var keyPos = new Vector2(x + cardWidth - keySize.X - 20, y + cardHeight - keySize.Y - 20);
                DrawTextWithShadow(spriteBatch, keyText, keyPos, Color.Cyan);
            }
            
            var instruction = "Press 1-3 to select a perk";
            var instructionSize = _font.MeasureString(instruction);
            var instructionPos = new Vector2((screenWidth - instructionSize.X) / 2, y + cardHeight + 50);
            DrawTextWithShadow(spriteBatch, instruction, instructionPos, Color.LightGray);
        }
        
        private void DrawTextWithShadow(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(_font, text, position + new Vector2(1, 1), Color.Black * 0.5f);
            spriteBatch.DrawString(_font, text, position, color);
        }
        
        private string WrapText(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var words = text.Split(' ');
            var wrappedText = "";
            var line = "";
            
            foreach (var word in words)
            {
                var testLine = line + (line.Length == 0 ? "" : " ") + word;
                var size = _font.MeasureString(testLine);
                
                if (size.X <= maxWidth)
                {
                    line = testLine;
                }
                else
                {
                    if (line.Length > 0)
                    {
                        wrappedText += line + "\n";
                        line = word;
                    }
                    else
                    {
                        wrappedText += word + "\n";
                        line = "";
                    }
                }
            }
            
            if (line.Length > 0)
            {
                wrappedText += line;
            }
            
            return wrappedText;
        }
    }
}
