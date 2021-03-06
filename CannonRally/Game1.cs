﻿using System;
using System.Linq;
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
using MonoGame.Extended.Shapes;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

namespace CannonRally
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private readonly FramesPerSecondCounter _fpsCounter;
        private readonly GraphicsDeviceManager _graphics;
        private Camera2D _camera;
        private Car _car;
        private Car _enemyCar;
        private DebugViewXNA _debugView;

        private SpriteFont _font;
        private IMapRenderer _mapRenderer;

        private KeyboardState _oldKeyboardState = Keyboard.GetState();
        private SpriteBatch _spriteBatch;

        private TiledMap _tiledMap;
        private Vector3 _translateCenter;
        private World _world;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _fpsCounter = new FramesPerSecondCounter();
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Window.AllowUserResizing = true;

            _world = new World(Vector2.Zero)
            {
                ContactManager =
                {
                    BeginContact = BeginContact,
                    EndContact = EndContact
                }
            };
            ConvertUnits.SetDisplayUnitToSimUnitRatio(32);
            _debugView = _debugView ?? new DebugViewXNA(_world);
            _debugView.AppendFlags(DebugViewFlags.DebugPanel);
            _debugView.Enabled = false;

            ViewportAdapter viewportAdapter = new BoxingViewportAdapter(Window,
                                                                        GraphicsDevice,
                                                                        GraphicsDevice.Viewport.Width*2,
                                                                        GraphicsDevice.Viewport.Height*2);
            _camera = new Camera2D(viewportAdapter);

            _translateCenter =
                new Vector3(
                    new Vector2(ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Width / 2f),
                                ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Height / 2f)),
                    0f);

            _mapRenderer = new FullMapRenderer(GraphicsDevice);
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

            var groundSnow = BodyFactory.CreateCircle(_world, 10f, 0, userData: new GroundAreaUserData(0.5f, false));
            groundSnow.IsSensor = true;
            var groundIce = BodyFactory.CreateCircle(_world,
                                              1f,
                                              0,
                                              new Vector2(10f),
                                              userData: new GroundAreaUserData(0.2f, false));
            groundIce.IsSensor = true;

            _font = Content.Load<SpriteFont>("Font");
            _debugView.LoadContent(GraphicsDevice, Content);

            _tiledMap = Content.Load<TiledMap>("level01");
            _mapRenderer.SwapMap(_tiledMap);
            var path = GetPath(_tiledMap, "path");

            var tireSprite = new Sprite(Content.Load<Texture2D>("tire"));
            var carSprite = new Sprite(Content.Load<Texture2D>("car_yellow_5"));
            _car = new Car(_world, carSprite, tireSprite);
            _car.ResetPosition(new Vector2(26f, 46f), -MathHelper.PiOver2);
            _car.CarBehavior = new ManualCarBehavior(_car);

            var greenCarSprite = new Sprite(Content.Load<Texture2D>("car_green_2"));
            _enemyCar = new Car(_world, greenCarSprite, tireSprite);
            _enemyCar.ResetPosition(new Vector2(28f, 49f), -MathHelper.PiOver2);
            _enemyCar.CarBehavior = new PathFollowerCarBehavior(_enemyCar, path);
        }

        private static Path GetPath(TiledMap tiledMap, string pathLayerName)
        {
            var pathPolyline = (PolylineF) tiledMap?.GetObjectGroup(pathLayerName)?.Objects?.First()?.Shape;

            if ((pathPolyline == null) || !pathPolyline.Points.Any())
            {
                throw new Exception("Level path not found.");
            }
            return new Path(pathPolyline.Points.Select(ConvertUnits.ToSimUnits).ToArray());
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
            {
                return;
            }

            if ((fudA.Type == FixtureUserDataType.CarTire) && (fudB.Type == FixtureUserDataType.GroundArea))
            {
                TireVsGroundArea(contact.FixtureA, contact.FixtureB, began);
            }
            else if ((fudA.Type == FixtureUserDataType.GroundArea) && (fudB.Type == FixtureUserDataType.CarTire))
            {
                TireVsGroundArea(contact.FixtureB, contact.FixtureA, began);
            }
        }

        private void TireVsGroundArea(Fixture tireFixture, Fixture groundFixture, bool began)
        {
            var tire = (Tire) tireFixture.Body.UserData;
            if (began)
            {
                tire.AddGroundArea((GroundAreaUserData) groundFixture.UserData);
            }
            else
            {
                tire.RemoveGroundArea((GroundAreaUserData) groundFixture.UserData);
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
            var keyboardState = Keyboard.GetState();
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
                keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (keyboardState.IsKeyDown(Keys.F10) && _oldKeyboardState.IsKeyUp(Keys.F10))
            {
                _debugView.Enabled = !_debugView.Enabled;
            }

            _world.Step(Math.Min((float) gameTime.ElapsedGameTime.TotalSeconds, 1f / 30f));

            _car.Update(gameTime);
            _enemyCar.Update(gameTime);
            _camera.LookAt(ConvertUnits.ToDisplayUnits(_car.Body.Position));

            _fpsCounter.Update(gameTime);

            _oldKeyboardState = keyboardState;
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

            _mapRenderer.Draw(transformMatrix);

            _spriteBatch.Begin(transformMatrix: transformMatrix);
            _car.Draw(_spriteBatch);
            _enemyCar.Draw(_spriteBatch);
            _spriteBatch.End();

            RenderSimulationDebugView();

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, $"FPS: {_fpsCounter.FramesPerSecond}", new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(_font, $"CAR SIM POS: {_car.Body.Position}", new Vector2(0, 20), Color.White);
            _spriteBatch.DrawString(_font, $"CAR POS: {ConvertUnits.ToDisplayUnits(_car.Body.Position)}", new Vector2(0, 40), Color.White);
            _spriteBatch.End();

            _fpsCounter.Draw(gameTime);

            base.Draw(gameTime);
        }

        private void RenderSimulationDebugView()
        {
            _camera.GetViewMatrix(Vector2.Zero);
            var projection = Matrix.CreateOrthographicOffCenter(
                0f,
                ConvertUnits.ToSimUnits(_graphics.GraphicsDevice.Viewport.Width * 2),
                ConvertUnits.ToSimUnits(_graphics.GraphicsDevice.Viewport.Height * 2),
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
            var translateBody = new Vector3(-new Vector2(ConvertUnits.ToSimUnits(_camera.Position.X),
                                                         ConvertUnits.ToSimUnits(_camera.Position.Y)),
                                            0f);

            return Matrix.CreateTranslation(-_translateCenter + translateBody) * matRotation * matZoom *
                   Matrix.CreateTranslation(_translateCenter);
        }
    }
}