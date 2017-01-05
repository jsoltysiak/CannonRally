using System;
using CannonRally.FixtureUserData;
using FarseerPhysics;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace CannonRally
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private DebugViewXNA _debugView;
        private SpriteBatch _spriteBatch;
        private Car _car;
        private World _world;
        private Camera2D _camera;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
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
            _world = new World(Vector2.Zero)
            {
                ContactManager =
                {
                    BeginContact = BeginContact,
                    EndContact = EndContact
                }
            };

            _debugView = _debugView ?? new DebugViewXNA(_world);

            ViewportAdapter viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 800, 480);
            _camera = new Camera2D(viewportAdapter);
            _camera.ZoomOut(0.7f);
            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var tireSprite = new Sprite(Content.Load<Texture2D>("tire"));
            _car = new Car(_world, tireSprite) {Body = {Position = new Vector2(1f, 1f)}};

            var ground = BodyFactory.CreateCircle(_world, 3f, 0, userData: new GroundAreaUserData(0.5f, false));
            ground.IsSensor = true;

            _debugView.LoadContent(GraphicsDevice, Content);
        }

        private void EndContact(Contact contact)
        {
            HandleContact(contact, false);
        }

        private bool BeginContact(Contact contact)
        {
            HandleContact(contact, true);
            return true;
        }

        private void HandleContact(Contact contact, bool began)
        {
            var fudA = (FixtureUserData.FixtureUserData) contact.FixtureA.UserData;
            var fudB = (FixtureUserData.FixtureUserData) contact.FixtureB.UserData;

            if ((fudA == null) || (fudB == null))
                return;

            if ((fudA.Type == FixtureUserDataType.CarTire) && (fudB.Type == FixtureUserDataType.GroundArea))
                TireVsGroundArea(contact.FixtureA, contact.FixtureB, began);
            else if ((fudA.Type == FixtureUserDataType.GroundArea) && (fudB.Type == FixtureUserDataType.CarTire))
                TireVsGroundArea(contact.FixtureB, contact.FixtureA, began);
        }

        private void TireVsGroundArea(Fixture tireFixture, Fixture groundFixture, bool began)
        {
            var tire = (Tire) tireFixture.Body.UserData;
            if (began)
                tire.AddGroundArea((GroundAreaUserData) groundFixture.UserData);
            else
                tire.RemoveGroundArea((GroundAreaUserData) groundFixture.UserData);
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

            _world.Step(Math.Min((float) gameTime.ElapsedGameTime.TotalSeconds, 1f/30f));

            _car.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var transformMatrix = _camera.GetViewMatrix();
            _spriteBatch.Begin(transformMatrix: transformMatrix);

            var projection = Matrix.CreateOrthographicOffCenter(
                0f,
                ConvertUnits.ToSimUnits(_graphics.GraphicsDevice.Viewport.Width),
                ConvertUnits.ToSimUnits(_graphics.GraphicsDevice.Viewport.Height),
                0f,
                0f,
                1f);

            _car.Draw(_spriteBatch);
            _spriteBatch.End();

            Matrix matRotation = Matrix.CreateRotationZ(_camera.Rotation);
            Matrix matZoom = Matrix.CreateScale(_camera.Zoom);
            Vector3 translateCenter = new Vector3(new Vector2(ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Width / 2f), ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Height / 2f)), 0f);
            Vector3 translateBody = new Vector3(-new Vector2(ConvertUnits.ToSimUnits(_camera.Position.X + GraphicsDevice.Viewport.Width / 2f), ConvertUnits.ToSimUnits(_camera.Position.Y + GraphicsDevice.Viewport.Height / 2f)), 0f);

            var SimView = Matrix.CreateTranslation(translateBody) * matRotation * matZoom * Matrix.CreateTranslation(translateCenter);

            _debugView.RenderDebugData(projection, SimView);
            base.Draw(gameTime);
        }
    }
}