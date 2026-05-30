using ComboArena.Controller;
using ComboArena.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ComboArena
{
    /// <summary>
    /// Главный класс игры, наследуемый от MonoGame Game.
    /// Отвечает за инициализацию графики, загрузку контента,
    /// главный игровой цикл (Update/Draw) и управление камерой.
    /// </summary>
    public class Game1 : Game
    {
        /// <summary>Менеджер графических устройств (разрешение, полноэкранность).</summary>
        private GraphicsDeviceManager _graphics;

        /// <summary>Спрайтовый пакет для отрисовки 2D-графики.</summary>
        private SpriteBatch _spriteBatch;

        /// <summary>Контроллер игры - управляет всей игровой логикой.</summary>
        private GameController _controller;

        /// <summary>Отображение игры - отвечает за отрисовку всех объектов и UI.</summary>
        private GameView _view;

        /// <summary>Шрифт для отрисовки текста в UI.</summary>
        private SpriteFont _font;

        /// <summary>Текстура фона (замощённая).</summary>
        private Texture2D _backgroundTexture;

        /// <summary>
        /// Создаёт игру с разрешением 1920x1080 в полноэкранном режиме.
        /// Инициализирует контроллер и устанавливает корневую директорию контента.
        /// </summary>
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _controller = new GameController();
            Mouse.GetState();
        }
        
        /// <summary>
        /// Загружает все необходимые текстуры, шрифты и инициализирует отображение.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("NFPixels");

            _backgroundTexture = Content.Load<Texture2D>("background_light");

            _view = new GameView(_controller.Player, _controller.Enemies, _controller.DropItems, _controller.OfferedPerks);
            _view.LoadContent(GraphicsDevice, _font, Content);
        }
        
        /// <summary>
        /// Главный метод обновления, вызываемый каждый кадр.
        /// Обрабатывает: выход по Escape, выбор перка (1-3),
        /// и обновление всей игровой логики через контроллер.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Выход по Escape
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Обработка выбора перка (клавиши 1-3)
            if (_controller.IsChoosingPerk)
            {
                var keyboard = Keyboard.GetState();
                if (keyboard.IsKeyDown(Keys.D1))
                    _controller.SelectPerk(0);
                else if (keyboard.IsKeyDown(Keys.D2))
                    _controller.SelectPerk(1);
                else if (keyboard.IsKeyDown(Keys.D3))
                    _controller.SelectPerk(2);
            }

            _controller.Update(gameTime);
            base.Update(gameTime);
        }
        
        /// <summary>
        /// Главный метод отрисовки, вызываемый каждый кадр.
        /// Реализует камеру, следящую за игроком (с зумом 2x),
        /// рисует замощённый фон, игровые объекты и UI поверх.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

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
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
