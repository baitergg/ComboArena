using ComboArena.Controller;
using ComboArena.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
namespace ComboArena
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameController _controller;
        private GameView _view;
        private SpriteFont _font;
        private Texture2D _backgroundTexture;
        private int _backgroundWidth;
        private int _backgroundHeight;
        private MouseState _previousMouseState;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _controller = new GameController();
            _previousMouseState = Mouse.GetState();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("NFPixels");
            
            _backgroundTexture = Content.Load<Texture2D>("background_light");
            
            _backgroundWidth = _backgroundTexture.Width;
            _backgroundHeight = _backgroundTexture.Height;
            _view = new GameView(_controller);
            _view.LoadContent(GraphicsDevice, _font, Content);
        }
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            var currentMouseState = Mouse.GetState();
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
            _previousMouseState = currentMouseState;
            _controller.Update(gameTime);
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            var viewport = GraphicsDevice.Viewport;
            const float zoom = 2.0f;
            var playerPos = _controller.Player.Position;
            var screenCenter = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            var playerCenter = playerPos + new Vector2(_controller.Player.Width / 2,
                                                       _controller.Player.Height / 2);
            var viewMatrix = Matrix.CreateTranslation(-playerCenter.X, -playerCenter.Y, 0) *
                             Matrix.CreateScale(zoom, zoom, 1) *
                             Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0);
            var visibleLeft = playerCenter.X - viewport.Width / (2 * zoom);
            var visibleTop = playerCenter.Y - viewport.Height / (2 * zoom);
            var visibleRight = visibleLeft + viewport.Width / zoom;
            var visibleBottom = visibleTop + viewport.Height / zoom;
            _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.LinearWrap);
            
            var backgroundRect = new Rectangle(
                (int)visibleLeft,
                (int)visibleTop,
                (int)(visibleRight - visibleLeft),
                (int)(visibleBottom - visibleTop));
            
            _spriteBatch.Draw(_backgroundTexture, backgroundRect, backgroundRect, Color.White);
            
            _view.Draw(_spriteBatch);
            _spriteBatch.End();
            _spriteBatch.Begin();
            _view.DrawUI(_spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
