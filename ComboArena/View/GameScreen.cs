using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ComboArena.View
{
    /// <summary>
    /// Состояние экрана: главное меню, игра, пауза, Game Over.
    /// </summary>
    public enum ScreenState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Instructions
    }

    /// <summary>
    /// Отвечает за отрисовку и обработку ввода на экранах
    /// главного меню, паузы и Game Over.
    /// </summary>
    public class GameScreen
    {
        public ScreenState State { get; set; } = ScreenState.MainMenu;

        private bool IsFullscreen { get; set; } = true;

        public event System.Action<bool> OnFullscreenToggle;

        public event System.Action OnExit;

        public event System.Action OnRestart;

        public event System.Action OnStartGame;

        public event System.Action OnReturnToMenu;

        private SpriteFont _font;

        private Texture2D _pixelTexture;

        private Texture2D _menuBackground;

        private KeyboardState _previousKeyboard;

        private int _selectedIndex;

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font, Texture2D menuBackground = null)
        {
            _font = font;
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            _menuBackground = menuBackground;
        }

        public void Update()
        {
            var keyboard = Keyboard.GetState();

            // Выход по Escape из любого экрана (кроме GameOver - там свои кнопки)
            if (State == ScreenState.Paused && IsKeyPressed(keyboard, Keys.Escape))
            {
                State = ScreenState.Playing;
            }
            else if (State == ScreenState.MainMenu)
            {
                UpdateMainMenu(keyboard);
            }
            else if (State == ScreenState.Paused)
            {
                UpdatePaused(keyboard);
            }
            else if (State == ScreenState.GameOver)
            {
                UpdateGameOver(keyboard);
            }
            else if (State == ScreenState.Instructions)
            {
                UpdateInstructions(keyboard);
            }

            _previousKeyboard = keyboard;
        }

        private void UpdateMainMenu(KeyboardState keyboard)
        {
            const int itemCount = 4;

            // Навигация вверх/вниз
            if (IsKeyPressed(keyboard, Keys.W) || IsKeyPressed(keyboard, Keys.Up))
                _selectedIndex = (_selectedIndex - 1 + itemCount) % itemCount;
            if (IsKeyPressed(keyboard, Keys.S) || IsKeyPressed(keyboard, Keys.Down))
                _selectedIndex = (_selectedIndex + 1) % itemCount;

            // Выбор пункта
            if (IsKeyPressed(keyboard, Keys.Enter) || IsKeyPressed(keyboard, Keys.Space))
            {
                if (_selectedIndex == 0)
                {
                    State = ScreenState.Playing;
                    OnStartGame?.Invoke();
                }
                else if (_selectedIndex == 1)
                {
                    IsFullscreen = !IsFullscreen;
                    OnFullscreenToggle?.Invoke(IsFullscreen);
                }
                else if (_selectedIndex == 2)
                {
                    State = ScreenState.Instructions;
                }
                else if (_selectedIndex == 3)
                {
                    OnExit?.Invoke();
                }
            }
        }

        private void UpdatePaused(KeyboardState keyboard)
        {
            if (IsKeyPressed(keyboard, Keys.W) || IsKeyPressed(keyboard, Keys.Up))
                _selectedIndex = (_selectedIndex - 1 + 2) % 2;
            if (IsKeyPressed(keyboard, Keys.S) || IsKeyPressed(keyboard, Keys.Down))
                _selectedIndex = (_selectedIndex + 1) % 2;

            if (IsKeyPressed(keyboard, Keys.Enter) || IsKeyPressed(keyboard, Keys.Space))
            {
                if (_selectedIndex == 0)
                {
                    State = ScreenState.Playing;
                }
                else if (_selectedIndex == 1)
                {
                    OnReturnToMenu?.Invoke();
                }
            }
        }

        private void UpdateGameOver(KeyboardState keyboard)
        {
            if (IsKeyPressed(keyboard, Keys.W) || IsKeyPressed(keyboard, Keys.Up))
                _selectedIndex = (_selectedIndex - 1 + 2) % 2;
            if (IsKeyPressed(keyboard, Keys.S) || IsKeyPressed(keyboard, Keys.Down))
                _selectedIndex = (_selectedIndex + 1) % 2;

            if (IsKeyPressed(keyboard, Keys.Enter) || IsKeyPressed(keyboard, Keys.Space))
            {
                if (_selectedIndex == 0)
                {
                    OnRestart?.Invoke();
                }
                else if (_selectedIndex == 1)
                {
                    OnReturnToMenu?.Invoke();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (State == ScreenState.MainMenu)
                DrawMainMenu(spriteBatch);
            else if (State == ScreenState.Paused)
                DrawPaused(spriteBatch);
            else if (State == ScreenState.GameOver)
                DrawGameOver(spriteBatch);
            else if (State == ScreenState.Instructions)
                DrawInstructions(spriteBatch);
        }

        private void DrawMainMenu(SpriteBatch spriteBatch)
        {
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            var centerX = viewport.Width / 2f;
            var centerY = viewport.Height / 2f;

            // Фон главного меню (растягивается на весь экран)
            spriteBatch.Draw(_menuBackground, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);

            // Пункты меню
            var menuItems = new[] { "Start Game", IsFullscreen ? "Fullscreen: ON" : "Fullscreen: OFF", "Instructions", "Exit Game" };
            DrawMenuItems(spriteBatch, menuItems, centerX, centerY - 40, 55);
        }

        private void DrawPaused(SpriteBatch spriteBatch)
        {
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            var centerX = viewport.Width / 2f;
            var centerY = viewport.Height / 2f;

            // Полупрозрачный затемнитель
            spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, viewport.Width, viewport.Height),
                new Color(0, 0, 0, 150));

            // Заголовок
            const string title = "PAUSED";
            DrawCenteredText(spriteBatch, title, centerX, centerY - 100, Color.White, 1.5f);

            // Пункты
            var menuItems = new[] { "Continue", "Exit to Menu" };
            DrawMenuItems(spriteBatch, menuItems, centerX, centerY, 60);
        }

        private void DrawGameOver(SpriteBatch spriteBatch)
        {
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            var centerX = viewport.Width / 2f;
            var centerY = viewport.Height / 2f;

            // Полупрозрачный затемнитель 
            spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, viewport.Width, viewport.Height),
                new Color(0, 0, 0, 150));

            // Заголовок
            const string title = "GAME OVER";
            DrawCenteredText(spriteBatch, title, centerX, centerY - 100, Color.Red, 1.8f);

            // Пункты
            var menuItems = new[] { "Restart", "Exit to Menu" };
            DrawMenuItems(spriteBatch, menuItems, centerX, centerY, 60);
        }

        private void DrawMenuItems(SpriteBatch spriteBatch, string[] items, float centerX, float startY, float spacing)
        {
            for (var i = 0; i < items.Length; i++)
            {
                var isSelected = i == _selectedIndex;
                var color = isSelected ? Color.Yellow : Color.White;
                var scale = isSelected ? 2.2f : 1.8f;

                // Маркер выбранного пункта
                if (isSelected)
                {
                    const string marker = "> ";
                    var markerSize = _font.MeasureString(marker) * scale;
                    var textSize = _font.MeasureString(items[i]) * scale;
                    var totalWidth = markerSize.X + textSize.X;
                    var startX = centerX - totalWidth / 2;
                    DrawCenteredText(spriteBatch, marker, startX + markerSize.X / 2, startY + i * spacing, Color.Yellow, scale);
                    DrawCenteredText(spriteBatch, items[i], startX + markerSize.X + textSize.X / 2, startY + i * spacing, color, scale);
                }
                else
                {
                    DrawCenteredText(spriteBatch, items[i], centerX, startY + i * spacing, color, scale);
                }
            }
        }

        private void DrawCenteredText(SpriteBatch spriteBatch, string text, float centerX, float y, Color color, float scale = 1f)
        {
            var size = _font.MeasureString(text) * scale;
            var position = new Vector2(centerX - size.X / 2, y - size.Y / 2);
            spriteBatch.DrawString(_font, text, position + new Vector2(1, 1) * scale, Color.Black * 0.5f, 0f,
                Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        private void UpdateInstructions(KeyboardState keyboard)
        {
            if (IsKeyPressed(keyboard, Keys.Escape) || IsKeyPressed(keyboard, Keys.Enter) || IsKeyPressed(keyboard, Keys.Space))
            {
                State = ScreenState.MainMenu;
            }
        }

        private void DrawInstructions(SpriteBatch spriteBatch)
        {
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            var centerX = viewport.Width / 2f;

            spriteBatch.Draw(_menuBackground, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);

            // Полупрозрачный затемнитель поверх фона для читаемости текста
            spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, viewport.Width, viewport.Height),
                new Color(0, 0, 0, 180));

            DrawCenteredText(spriteBatch, "HOW TO PLAY", centerX, 50, Color.Gold, 2.2f);

            // Три колонки
            var colWidth = viewport.Width / 3f;
            var colCenters = new[]
            {
                colWidth / 2f,
                colWidth + colWidth / 2f,
                colWidth * 2 + colWidth / 2f
            };

            const float colStartY = 140f;
            const float lineSpacing = 38f;

            DrawCenteredText(spriteBatch, "ABOUT", colCenters[0], colStartY, Color.Gold, 1.5f);
            var aboutLines = new[]
            {
                "Survive waves of enemies",
                "and defeat them to earn",
                "experience and level up.",
                "Every 3 levels, choose",
                "a perk to upgrade",
                "your character.",
                "Build combos by defeating",
                "enemies quickly for",
                "bonus damage and XP."
            };
            var y = colStartY + 50f;
            foreach (var line in aboutLines)
            {
                DrawCenteredText(spriteBatch, line, colCenters[0], y, Color.White, 1.0f);
                y += lineSpacing;
            }

            DrawCenteredText(spriteBatch, "CONTROLS", colCenters[1], colStartY, Color.Gold, 1.5f);
            var controlsLines = new[]
            {
                "WASD / Arrows  -  Move",
                "Space  -  Attack",
                "E  -  Activate ability",
                "Escape  -  Pause",
                "1 / 2 / 3  -  Select perk"
            };
            y = colStartY + 50f;
            foreach (var line in controlsLines)
            {
                DrawCenteredText(spriteBatch, line, colCenters[1], y, Color.White, 1.0f);
                y += lineSpacing;
            }

            DrawCenteredText(spriteBatch, "TIPS", colCenters[2], colStartY, Color.Gold, 1.5f);
            var tipsLines = new[]
            {
                "Collect XP orbs and",
                "health hearts dropped",
                "by enemies.",
                "Choose perks wisely",
                "to match your playstyle.",
                "Higher combos =",
                "more damage and XP."
            };
            y = colStartY + 50f;
            foreach (var line in tipsLines)
            {
                DrawCenteredText(spriteBatch, line, colCenters[2], y, Color.White, 1.0f);
                y += lineSpacing;
            }

            DrawCenteredText(spriteBatch, "Press ESC, Enter or Space to return", centerX, viewport.Height - 60, Color.LightGray, 1.1f);
        }

        private bool IsKeyPressed(KeyboardState current, Keys key)
        {
            return current.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);
        }
    }
}