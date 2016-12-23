using System;
using FarseerPhysics;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CannonRally
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private SpriteFont _font;
        private Tire _tire;
        private DebugViewXNA DebugView;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private World world;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            world = new World(Vector2.Zero);

            var tireSprite = new Sprite(Content.Load<Texture2D>("tire"));
            _tire = new Tire(BodyFactory.CreateRectangle(world, 5f, 5f, 1f, new Vector2(1, 2)), tireSprite);

            if (DebugView == null)
            {
                DebugView = new DebugViewXNA(world);
                DebugView.RemoveFlags(DebugViewFlags.Shape);
                DebugView.RemoveFlags(DebugViewFlags.Joint);
                DebugView.DefaultShapeColor = Color.White;
                DebugView.SleepingShapeColor = Color.LightGray;
                DebugView.LoadContent(GraphicsDevice, Content);
            }
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            world.Step(Math.Min((float) gameTime.ElapsedGameTime.TotalSeconds, 1f/30f));
            _tire.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var SimProjection = Matrix.CreateOrthographicOffCenter(
                0f,
                ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Width),
                ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Height), 0f, 0f, 
                1f);
            var SimView = Matrix.Identity;
            var View = Matrix.Identity;
            var projection = Matrix.CreateOrthographicOffCenter(
                0f,
                ConvertUnits.ToSimUnits(graphics.GraphicsDevice.Viewport.Width),
                ConvertUnits.ToSimUnits(graphics.GraphicsDevice.Viewport.Height), 0f, 0f,
                1f);

            spriteBatch.Begin(0, null, null, null, null, null, View);
            //spriteBatch.Begin();
            _tire.Draw(spriteBatch);


            DebugView.RenderDebugData(ref projection);

            spriteBatch.End();
            //DebugView.RenderDebugData(SimProjection, SimView);


            base.Draw(gameTime);
        }
    }
}