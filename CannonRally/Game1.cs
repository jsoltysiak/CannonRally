﻿using System;
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
using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;

namespace CannonRally
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private Camera2D _camera;
        private Car _car;
        private DebugViewXNA _debugView;
        private SpriteBatch _spriteBatch;
        private Vector3 _translateCenter;
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

            _translateCenter =
                new Vector3(
                    new Vector2(ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Width/2f),
                        ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Height/2f)), 0f);
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
            _car.Draw(_spriteBatch);
            _spriteBatch.End();

            RenderSimulationDebugView();

            base.Draw(gameTime);
        }

        private void RenderSimulationDebugView()
        {
            var projection = Matrix.CreateOrthographicOffCenter(
                0f,
                ConvertUnits.ToSimUnits(_graphics.GraphicsDevice.Viewport.Width),
                ConvertUnits.ToSimUnits(_graphics.GraphicsDevice.Viewport.Height),
                0f,
                0f,
                1f);


            var simView = GetSimulationView();

            _debugView.RenderDebugData(projection, simView);
        }

        private Matrix GetSimulationView()
        {
            var matRotation = Matrix.CreateRotationZ(_camera.Rotation);
            var matZoom = Matrix.CreateScale(_camera.Zoom);
            var translateBody =
                new Vector3(
                    -new Vector2(ConvertUnits.ToSimUnits(_camera.Position.X),
                        ConvertUnits.ToSimUnits(_camera.Position.Y)), 0f);

            return Matrix.CreateTranslation(-_translateCenter + translateBody)*matRotation*matZoom*
                   Matrix.CreateTranslation(_translateCenter);
        }
    }
}