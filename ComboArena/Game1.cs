using ComboArena.Controller;
using ComboArena.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComboArena
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameController _controller;
        private GameView _view;
        private SpriteFont _font;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            
            _graphics.IsFullScreen = true;
            
            _controller = new GameController();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            _font = null;
            
            _view = new GameView(_controller);
            _view.LoadContent(GraphicsDevice, _font);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();

            _controller.Update(gameTime);
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            const float zoom = 2.0f;
            var playerPos = _controller.Player.Position;
            var viewport = GraphicsDevice.Viewport;
            var screenCenter = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            var playerCenter = playerPos + new Vector2(_controller.Player.Width / 2,
                                                       _controller.Player.Height / 2);
            
            var viewMatrix = Matrix.CreateTranslation(-playerCenter.X, -playerCenter.Y, 0) *
                             Matrix.CreateScale(zoom, zoom, 1) *
                             Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0);
            
            _spriteBatch.Begin(transformMatrix: viewMatrix);
            _view.Draw(_spriteBatch);
            _spriteBatch.End();
            
            _spriteBatch.Begin();
            _view.DrawUI(_spriteBatch);
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}