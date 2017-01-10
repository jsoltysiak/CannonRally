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
using MonoGame.Extended.Sprites;

namespace CannonRally
{
    public class Car
    {
        public const float LockAngle = 40f * MathHelper.Pi / 180f;
        public const float TurnSpeedPerSec = 320 * MathHelper.Pi / 180f;
        public const float TurnPerTimeStep = TurnSpeedPerSec / 60f;
        private readonly RevoluteJoint _frontLeftJoint;
        private readonly RevoluteJoint _frontRightJoint;

        private readonly Sprite _hullSprite;
        public readonly IList<Tire> FrontTires;
        public readonly IList<Tire> RearTires;

        public Car(World world, Sprite hullSprite, Sprite tireSprite)
        {
            _hullSprite = hullSprite;

            Body = BodyFactory.CreateRoundedRectangle(
                world,
                ConvertUnits.ToSimUnits(_hullSprite.TextureRegion.Width),
                ConvertUnits.ToSimUnits(_hullSprite.TextureRegion.Height),
                ConvertUnits.ToSimUnits(_hullSprite.TextureRegion.Width) / 4,
                ConvertUnits.ToSimUnits(_hullSprite.TextureRegion.Height) / 4,
                2,
                0.3f,
                bodyType: BodyType.Dynamic);

            FrontTires = new List<Tire>();
            var tire = CreateTire(world, tireSprite);
            FrontTires.Add(tire);
            var joint = CreateWheelJoint(world, tire.Body, new Vector2(-0.7f, -1f));
            _frontRightJoint = joint;

            tire = CreateTire(world, tireSprite);
            FrontTires.Add(tire);
            joint = CreateWheelJoint(world, tire.Body, new Vector2(0.7f, -1f));
            _frontLeftJoint = joint;

            RearTires = new List<Tire>();
            tire = CreateTire(world, tireSprite);
            RearTires.Add(tire);
            CreateWheelJoint(world, tire.Body, new Vector2(0.7f, 1));

            tire = CreateTire(world, tireSprite);
            RearTires.Add(tire);
            CreateWheelJoint(world, tire.Body, new Vector2(-0.7f, 1f));
        }

        public float MaxForwardSpeed { get; } = 5f;
        public float MaxBackwardSpeed { get; } = -5f;

        public ICarBehavior CarBehavior { get; set; }

        public Body Body { get; }

        private RevoluteJoint CreateWheelJoint(World world, Body tireBody, Vector2 anchorPosition)
        {
            var joint = JointFactory.CreateRevoluteJoint(world, Body, tireBody, anchorPosition, Body.LocalCenter);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);
            return joint;
        }

        private static Tire CreateTire(World world, Sprite tireSprite)
        {
            return new Tire(
                BodyFactory.CreateRoundedRectangle(
                    world,
                    ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Width),
                    ConvertUnits.ToSimUnits(tireSprite.TextureRegion.Height),
                    0.02f,
                    0.02f,
                    0,
                    1f,
                    userData: new TireUserData()),
                tireSprite);
        }

        public void Update(GameTime gameTime)
        {
            if (CarBehavior != null)
            {
                var desiredAngle = CarBehavior.GetDesiredWheelAngle();

                var angleNow = _frontLeftJoint.JointAngle;
                var angleToTurn = desiredAngle - angleNow;
                angleToTurn = MathUtils.Clamp(angleToTurn, -TurnPerTimeStep, TurnPerTimeStep);
                var newAngle = angleNow + angleToTurn;
                _frontLeftJoint.SetLimits(newAngle, newAngle);
                _frontRightJoint.SetLimits(newAngle, newAngle);

                foreach (var frontTire in FrontTires)
                {
                    frontTire.Update(gameTime);
                    UpdateDrive(frontTire);
                }
                foreach (var rearTire in RearTires)
                {
                    rearTire.Update(gameTime);
                    UpdateDrive(rearTire);
                }
            }

            _hullSprite.Rotation = Body.Rotation;
            _hullSprite.Position = ConvertUnits.ToDisplayUnits(Body.Position);
        }

        private void UpdateDrive(Tire tire)
        {
            var desiredSpeed = CarBehavior.GetDesiredSpeed();

            if (Math.Abs(desiredSpeed) > float.Epsilon)
            {
                var currentForwardNormal = tire.Body.GetWorldVector(new Vector2(0, -1));
                var currentSpeed = Vector2.Dot(tire.GetForwardVelocity(), currentForwardNormal);

                float force = 0;
                if (desiredSpeed > currentSpeed)
                {
                    force = tire.MaxDriveForce;
                }
                else if (desiredSpeed < currentSpeed)
                {
                    force = -tire.MaxDriveForce;
                }
                else
                {
                    return;
                }

                tire.Body.ApplyForce(force * currentForwardNormal, Body.WorldCenter);
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            foreach (var frontTire in FrontTires)
            {
                frontTire.Draw(spriteBatch);
            }
            foreach (var rearTire in RearTires)
            {
                rearTire.Draw(spriteBatch);
            }

            _hullSprite.Draw(spriteBatch);
        }
    }
}