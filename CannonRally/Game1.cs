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
        private Tire _tire;
        private World _world;

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
            _world = new World(Vector2.Zero);
            _world.ContactManager.BeginContact = BeginContact;
            _world.ContactManager.EndContact = EndContact;

            if (_debugView == null)
            {
                _debugView = new DebugViewXNA(_world);
                _debugView.RemoveFlags(DebugViewFlags.Controllers);
                _debugView.RemoveFlags(DebugViewFlags.Joint);
                _debugView.AppendFlags(DebugViewFlags.PolygonPoints);
                _debugView.AppendFlags(DebugViewFlags.DebugPanel);
            }

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
            _tire =
                new Tire(
                    BodyFactory.CreateRoundedRectangle(_world, ConvertUnits.ToSimUnits(tireSprite.Texture.Width),
                        ConvertUnits.ToSimUnits(tireSprite.Texture.Height), 0.1f, 0.1f, 0, 1f, new Vector2(1, 2),
                        userData: new TireUserData()), tireSprite);

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

            _spriteBatch.Begin();

            var projection = Matrix.CreateOrthographicOffCenter(
                0f,
                ConvertUnits.ToSimUnits(_graphics.GraphicsDevice.Viewport.Width),
                ConvertUnits.ToSimUnits(_graphics.GraphicsDevice.Viewport.Height), 0f, 0f,
                1f);
            _tire.Draw(_spriteBatch);
            _spriteBatch.End();
            _debugView.RenderDebugData(ref projection);
            base.Draw(gameTime);
        }
    }
}