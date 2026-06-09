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
        Instructions,
        Tutorial
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

        private int _tutorialPage;

        private static readonly TutorialPage[] TutorialPages =
        {
            new TutorialPage
            {
                Title = "ДВИЖЕНИЕ",
                Lines = new[]
                {
                    "Используйте клавиши WASD или",
                    "Стрелки для перемещения",
                    "персонажа по полю.",
                    "",
                    "Двигайтесь, чтобы уклоняться",
                    "от врагов и собирать",
                    "предметы."
                },
                Icon = "WASD"
            },
            new TutorialPage
            {
                Title = "АТАКА",
                Lines = new[]
                {
                    "Нажмите ПРОБЕЛ для атаки.",
                    "",
                    "Атака имеет перезарядку -",
                    "следите за индикатором",
                    "над полоской здоровья."
                },
                Icon = "Space"
            },
            new TutorialPage
            {
                Title = "ОПЫТ И УРОВЕНЬ",
                Lines = new[]
                {
                    "Убивайте врагов, чтобы",
                    "получать опыт.",
                    "Из врагов выпадают сферы",
                    "опыта - подбирайте их.",
                    "",
                    "При повышении уровня",
                    "восстанавливается здоровье."
                },
                Icon = "EXP"
            },
            new TutorialPage
            {
                Title = "УЛУЧШЕНИЯ",
                Lines = new[]
                {
                    "Каждые 3 уровня появляется",
                    "выбор из 3 улучшений.",
                    "",
                    "Нажмите 1, 2 или 3 для",
                    "выбора улучшения.",
                    "",
                    "Улучшения делают персонажа",
                    "сильнее и выносливее."
                },
                Icon = "1-2-3"
            },
            new TutorialPage
            {
                Title = "СПОСОБНОСТЬ",
                Lines = new[]
                {
                    "Нажмите E для активации",
                    "особой способности.",
                    "",
                    "Способность имеет перезарядку",
                    "и меняется при получении",
                    "новых улучшений.",
                    "",
                    "Используйте её в критический",
                    "момент боя."
                },
                Icon = "E"
            },
            new TutorialPage
            {
                Title = "КОМБО",
                Lines = new[]
                {
                    "Убивайте врагов быстро один",
                    "за другим, чтобы накапливать",
                    "комбо.",
                    "",
                    "Чем выше комбо, тем больше",
                    "бонусного урона и опыта",
                    "вы получаете.",
                    "",
                    "Комбо сбрасывается, если",
                    "долго не атаковать."
                },
                Icon = "Combo"
            },
            new TutorialPage
            {
                Title = "ЗДОРОВЬЕ И ПАУЗА",
                Lines = new[]
                {
                    "Собирайте сердечки здоровья,",
                    "выпадающие из врагов.",
                    "",
                    "Нажмите ESCAPE для паузы.",
                    "В паузе можно продолжить",
                    "или выйти в главное меню.",
                    "",
                    "Не дайте врагам себя окружить",
                    "и следите за здоровьем!"
                },
                Icon = "HP"
            }
        };

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
            else if (State == ScreenState.Tutorial)
            {
                UpdateTutorial(keyboard);
            }

            _previousKeyboard = keyboard;
        }

        private void UpdateMainMenu(KeyboardState keyboard)
        {
            const int itemCount = 5;

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
                    _tutorialPage = 0;
                    State = ScreenState.Tutorial;
                }
                else if (_selectedIndex == 2)
                {
                    IsFullscreen = !IsFullscreen;
                    OnFullscreenToggle?.Invoke(IsFullscreen);
                }
                else if (_selectedIndex == 3)
                {
                    State = ScreenState.Instructions;
                }
                else if (_selectedIndex == 4)
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
            else if (State == ScreenState.Tutorial)
                DrawTutorial(spriteBatch);
        }

        private void DrawMainMenu(SpriteBatch spriteBatch)
        {
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            var centerX = viewport.Width / 2f;
            var centerY = viewport.Height / 2f;

            // Фон главного меню (растягивается на весь экран)
            spriteBatch.Draw(_menuBackground, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);

            // Пункты меню
            var menuItems = new[] { "Начать игру", "Обучение", IsFullscreen ? "Полный экран: ВКЛ" : "Полный экран: ВЫКЛ", "Управление", "Выйти" };
            DrawMenuItems(spriteBatch, menuItems, centerX, centerY - 60, 55);
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
            const string title = "ПАУЗА";
            DrawCenteredText(spriteBatch, title, centerX, centerY - 100, Color.White, 1.5f);

            // Пункты
            var menuItems = new[] { "Продолжить", "Выйти в меню" };
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
            const string title = "ИГРА ОКОНЧЕНА";
            DrawCenteredText(spriteBatch, title, centerX, centerY - 100, Color.Red, 1.8f);

            // Пункты
            var menuItems = new[] { "Заново", "Выйти в меню" };
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

            DrawCenteredText(spriteBatch, "КАК ИГРАТЬ", centerX, 40, Color.Gold, 2.0f);

            // Три колонки
            var colWidth = viewport.Width / 3f;
            var colCenters = new[]
            {
                colWidth / 2f,
                colWidth + colWidth / 2f,
                colWidth * 2 + colWidth / 2f
            };

            const float colStartY = 110f;
            const float lineSpacing = 36f;

            DrawCenteredText(spriteBatch, "ОБ ИГРЕ", colCenters[0], colStartY, Color.Gold, 1.5f);
            var aboutLines = new[]
            {
                "Выживайте среди волн врагов,",
                "побеждайте их, получайте",
                "опыт и повышайте уровень.",
                "Каждые 3 уровня выбирайте",
                "улучшение для персонажа.",
                "Стройте комбо, быстро убивая",
                "врагов - это даёт бонусный",
                "урон и дополнительный опыт."
            };
            var y = colStartY + 45f;
            foreach (var line in aboutLines)
            {
                DrawCenteredText(spriteBatch, line, colCenters[0], y, Color.White, 1.1f);
                y += lineSpacing;
            }

            DrawCenteredText(spriteBatch, "УПРАВЛЕНИЕ", colCenters[1], colStartY, Color.Gold, 1.5f);
            var controlsLines = new[]
            {
                "WASD / Стрелки - Движение",
                "Пробел - Атака",
                "E - Активировать способность",
                "Escape - Пауза",
                "1 / 2 / 3 - Выбрать улучшение"
            };
            y = colStartY + 45f;
            foreach (var line in controlsLines)
            {
                DrawCenteredText(spriteBatch, line, colCenters[1], y, Color.White, 1.1f);
                y += lineSpacing;
            }

            DrawCenteredText(spriteBatch, "СОВЕТЫ", colCenters[2], colStartY, Color.Gold, 1.5f);
            var tipsLines = new[]
            {
                "Собирайте сферы опыта и",
                "сердечки здоровья, выпадающие",
                "из поверженных врагов.",
                "Выбирайте улучшения с умом,",
                "подстраиваясь под свой стиль.",
                "Высокое комбо = больше",
                "урона и опыта."
            };
            y = colStartY + 45f;
            foreach (var line in tipsLines)
            {
                DrawCenteredText(spriteBatch, line, colCenters[2], y, Color.White, 1.1f);
                y += lineSpacing;
            }

            DrawCenteredText(spriteBatch, "ESC, Enter или Пробел - назад", centerX, viewport.Height - 50, Color.LightGray, 1.1f);
        }

        private void UpdateTutorial(KeyboardState keyboard)
        {
            if (IsKeyPressed(keyboard, Keys.Escape))
            {
                State = ScreenState.MainMenu;
            }
            else if (IsKeyPressed(keyboard, Keys.Enter) || IsKeyPressed(keyboard, Keys.Space))
            {
                if (_tutorialPage < TutorialPages.Length - 1)
                {
                    _tutorialPage++;
                }
                else
                {
                    State = ScreenState.MainMenu;
                }
            }
        }

        private void DrawTutorial(SpriteBatch spriteBatch)
        {
            var viewport = spriteBatch.GraphicsDevice.Viewport;
            var centerX = viewport.Width / 2f;

            // Фон
            spriteBatch.Draw(_menuBackground, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);

            // Затемнитель
            spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, viewport.Width, viewport.Height),
                new Color(0, 0, 0, 180));

            var page = TutorialPages[_tutorialPage];

            DrawCenteredText(spriteBatch, page.Title, centerX, 80, Color.Gold, 2.5f);

            DrawCenteredText(spriteBatch, $"[ {page.Icon} ]", centerX, 160, Color.CornflowerBlue, 1.8f);

            const float textStartY = 240f;
            const float lineSpacing = 50f;
            var y = textStartY;

            foreach (var line in page.Lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    y += lineSpacing * 0.5f;
                }
                else
                {
                    DrawCenteredText(spriteBatch, line, centerX, y, Color.White, 1.3f);
                    y += lineSpacing;
                }
            }

            var progressText = $"{_tutorialPage + 1} / {TutorialPages.Length}";
            DrawCenteredText(spriteBatch, progressText, centerX, viewport.Height - 100, Color.Gray, 1.2f);

            var isLastPage = _tutorialPage == TutorialPages.Length - 1;
            var hintText = isLastPage ? "Пробел или Enter - завершить" : "Пробел или Enter - далее";
            DrawCenteredText(spriteBatch, hintText, centerX, viewport.Height - 60, Color.LightGray, 1.1f);
            DrawCenteredText(spriteBatch, "Escape - в меню", centerX, viewport.Height - 30, Color.LightGray, 0.9f);
        }

        private bool IsKeyPressed(KeyboardState current, Keys key)
        {
            return current.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);
        }

        private struct TutorialPage
        {
            public string Title;
            public string[] Lines;
            public string Icon;
        }
    }
}