using System;
using System.Collections.Generic;
using CannonRally.FixtureUserData;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;

namespace CannonRally
{
    internal class Car
    {
        private const float LockAngle = 40f*MathHelper.Pi/180f;
        private const float TurnSpeedPerSec = 320*MathHelper.Pi/180f;
        private const float TurnPerTimeStep = TurnSpeedPerSec/60f;
        private readonly RevoluteJoint _frontLeftJoint;
        private readonly RevoluteJoint _frontRightJoint;
        public readonly IList<Tire> FrontTires;
        public readonly IList<Tire> RearTires;

        public Car(World world, Sprite tireSprite)
        {
            FrontTires = new List<Tire>();
            RearTires = new List<Tire>();

            var vertices = new Vertices
            {
                new Vector2(.5f, 0),
                new Vector2(.5f, 1f),
                new Vector2(.5f, 2f),
                new Vector2(.25f, 3),
                new Vector2(-.25f, 3),
                new Vector2(-.5f, 2f),
                new Vector2(-.5f, 1f),
                new Vector2(-.5f, 0)
            };

            Body = BodyFactory.CreatePolygon(world, vertices, 1.0f);
            Body.BodyType = BodyType.Dynamic;

            var tire = new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Width),
                    ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            var joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(-0.5f, 2.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);

            FrontTires.Add(tire);
            _frontRightJoint = joint;

            tire = new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Width),
                    ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(0.5f, 2.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);

            FrontTires.Add(tire);
            _frontLeftJoint = joint;

            tire = new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Width),
                    ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(0.5f, 0.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);

            RearTires.Add(tire);

            tire = new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Width),
                    ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(-0.5f, 0.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);

            RearTires.Add(tire);
        }

        public Body Body { get; }

        public void Update(GameTime gameTime)
        {
            float desiredAngle = 0;

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.A))
                desiredAngle = -LockAngle;
            else if (keyboardState.IsKeyDown(Keys.D))
                desiredAngle = LockAngle;

            var angleNow = _frontLeftJoint.JointAngle;
            var angleToTurn = desiredAngle - angleNow;
            angleToTurn = MathUtils.Clamp(angleToTurn, -TurnPerTimeStep, TurnPerTimeStep);
            var newAngle = angleNow + angleToTurn;
            _frontLeftJoint.SetLimits(newAngle, newAngle);
            _frontRightJoint.SetLimits(newAngle, newAngle);

            foreach (var frontTire in FrontTires)
                frontTire.Update(gameTime);
            foreach (var rearTire in RearTires)
                rearTire.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            foreach (var frontTire in FrontTires)
                frontTire.Draw(spriteBatch);
            foreach (var rearTire in RearTires)
                rearTire.Draw(spriteBatch);
        }
    }
}
