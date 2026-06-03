using ComboArena.Controller;
using ComboArena.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ComboArena.Core;

namespace ComboArena
{
    /// <summary>
    /// Главный класс игры, наследуемый от MonoGame Game.
    /// Отвечает за инициализацию графики, загрузку контента,
    /// главный игровой цикл (Update/Draw) и управление камерой.
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;

        private SpriteBatch _spriteBatch;

        private GameController _controller;

        private GameView _view;

        private readonly GameScreen _gameScreen;

        private SpriteFont _font;

        private Texture2D _backgroundTexture;

        private Texture2D _menuBackground;

        private KeyboardState _previousKeyboard;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _controller = new GameController();
            _gameScreen = new GameScreen();
            Mouse.GetState();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("NFPixels");

            _backgroundTexture = Content.Load<Texture2D>("background_light");
            _menuBackground = Content.Load<Texture2D>("main_menu");

            _view = new GameView(_controller.Player, _controller.Enemies, _controller.DropItems, _controller.OfferedPerks);
            _view.LoadContent(GraphicsDevice, _font, Content);

            _gameScreen.LoadContent(GraphicsDevice, _font, _menuBackground);

            // Подписка на события экрана
            _gameScreen.OnFullscreenToggle += ToggleFullscreen;
            _gameScreen.OnExit += () => Exit();
            _gameScreen.OnRestart += RestartGame;
            _gameScreen.OnStartGame += RestartGame;
            _gameScreen.OnReturnToMenu += ReturnToMenu;
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();

            // Обновление экрана (меню/пауза/Game Over)
            _gameScreen.Update();

            if (_gameScreen.State == ScreenState.Playing)
            {
                // Обработка выбора перка (клавиши 1-3)
                if (_controller.IsChoosingPerk)
                {
                    if (keyboard.IsKeyDown(Keys.D1))
                        _controller.SelectPerk(0);
                    else if (keyboard.IsKeyDown(Keys.D2))
                        _controller.SelectPerk(1);
                    else if (keyboard.IsKeyDown(Keys.D3))
                        _controller.SelectPerk(2);
                }

                // Пауза по Escape (однократное нажатие)
                if (keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
                {
                    _gameScreen.State = ScreenState.Paused;
                }

                _controller.Update(gameTime);

                // Проверка смерти игрока
                if (!_controller.Player.IsAlive)
                {
                    _gameScreen.State = ScreenState.GameOver;
                }
            }

            _previousKeyboard = keyboard;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (_gameScreen.State == ScreenState.MainMenu || _gameScreen.State == ScreenState.Instructions)
            {
                // Главное меню или экран инструкций
                _spriteBatch.Begin();
                _gameScreen.Draw(_spriteBatch);
                _spriteBatch.End();
            }
            else if (_gameScreen.State == ScreenState.Playing || _gameScreen.State == ScreenState.Paused || _gameScreen.State == ScreenState.GameOver)
            {
                // Игра, пауза или Game Over - рисуем мир
                var viewport = GraphicsDevice.Viewport;
                const float zoom = 2.0f;

                // Расчёт матрицы камеры: центр на игроке, зум 2x
                var playerPos = _controller.Player.Position;
                var screenCenter = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
                var playerCenter = playerPos + new Vector2(_controller.Player.Width / 2,
                                                                    _controller.Player.Height / 2);

                var viewMatrix = Matrix.CreateTranslation(-playerCenter.X, -playerCenter.Y, 0) *
                                 Matrix.CreateScale(zoom, zoom, 1) *
                                 Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0);

                // Расчёт видимой области для замощения фона
                var visibleLeft = playerCenter.X - viewport.Width / (2 * zoom);
                var visibleTop = playerCenter.Y - viewport.Height / (2 * zoom);
                var visibleRight = visibleLeft + viewport.Width / zoom;
                var visibleBottom = visibleTop + viewport.Height / zoom;

                // Отрисовка мира с камерой
                _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.LinearWrap);

                var backgroundRect = new Rectangle(
                    (int)visibleLeft,
                    (int)visibleTop,
                    (int)(visibleRight - visibleLeft),
                    (int)(visibleBottom - visibleTop));

                _spriteBatch.Draw(_backgroundTexture, backgroundRect, backgroundRect, Color.White);

                _view.Draw(_spriteBatch, gameTime);
                _spriteBatch.End();

                // Отрисовка UI без камеры (в экранных координатах)
                _spriteBatch.Begin();
                _view.DrawUI(_spriteBatch);

                // Если пауза или Game Over - рисуем поверх UI
                if (_gameScreen.State == ScreenState.Paused || _gameScreen.State == ScreenState.GameOver)
                {
                    _gameScreen.Draw(_spriteBatch);
                }

                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void ToggleFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                _graphics.PreferredBackBufferWidth = 1920;
                _graphics.PreferredBackBufferHeight = 1080;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = 1280;
                _graphics.PreferredBackBufferHeight = 720;
            }

            _graphics.IsFullScreen = fullscreen;
            _graphics.ApplyChanges();
        }

        private void RestartGame()
        {
            EventBus.Instance.Clear();
            _controller = new GameController();
            _view = new GameView(_controller.Player, _controller.Enemies, _controller.DropItems, _controller.OfferedPerks);
            _view.LoadContent(GraphicsDevice, _font, Content);
            _gameScreen.State = ScreenState.Playing;
        }

        private void ReturnToMenu()
        {
            _gameScreen.State = ScreenState.MainMenu;
        }
    }
}
