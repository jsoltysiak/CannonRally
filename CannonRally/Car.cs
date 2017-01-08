﻿using System.Collections.Generic;
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

        private readonly Sprite _hullSprite;
        public readonly IList<Tire> FrontTires;
        public readonly IList<Tire> RearTires;

        public Car(World world, Sprite hullSprite, Sprite tireSprite)
        {
            _hullSprite = hullSprite;

            Body = BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(_hullSprite.TextureRegion.Width),
                ConvertUnits.ToSimUnits(_hullSprite.TextureRegion.Height), 0.1f, 0.1f, 0, 1f, bodyType: BodyType.Dynamic);

            FrontTires = new List<Tire>();
            var tire = CreateTire(world, tireSprite);
            FrontTires.Add(tire);
            var joint = CreateWheelJoint(world, tire.Body, new Vector2(-0.15f, -0.18f));
            _frontRightJoint = joint;

            tire = CreateTire(world, tireSprite);
            FrontTires.Add(tire);
            joint = CreateWheelJoint(world, tire.Body, new Vector2(0.15f, -0.18f));
            _frontLeftJoint = joint;

            RearTires = new List<Tire>();
            tire = CreateTire(world, tireSprite);
            RearTires.Add(tire);
            CreateWheelJoint(world, tire.Body, new Vector2(0.15f, 0.18f));

            tire = CreateTire(world, tireSprite);
            RearTires.Add(tire);
            CreateWheelJoint(world, tire.Body, new Vector2(-0.15f, 0.18f));
        }

        public Body Body { get; }

        private RevoluteJoint CreateWheelJoint(World world, Body tireBody, Vector2 anchorPosition)
        {
            var joint = JointFactory.CreateRevoluteJoint(world, Body, tireBody, anchorPosition, Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);
            return joint;
        }

        private static Tire CreateTire(World world, Sprite tireSprite)
        {
            return new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Width),
                    ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Height), 0.02f, 0.02f, 0, 1f,
                    userData: new TireUserData()), tireSprite);
        }

        public void Update(GameTime gameTime)
        {
            float desiredAngle = 0;

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                desiredAngle = -LockAngle;
            else if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
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

            _hullSprite.Rotation = Body.Rotation;
            _hullSprite.Position = ConvertUnits.ToDisplayUnits(Body.Position);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            foreach (var frontTire in FrontTires)
                frontTire.Draw(spriteBatch);
            foreach (var rearTire in RearTires)
                rearTire.Draw(spriteBatch);

            _hullSprite.Draw(spriteBatch);
        }
    }
}