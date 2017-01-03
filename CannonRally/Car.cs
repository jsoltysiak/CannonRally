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

namespace CannonRally
{
    internal class Car
    {
        private const float LockAngle = 40f*MathHelper.Pi/180f;
        private const float TurnSpeedPerSec = 320*MathHelper.Pi/180f;
        private const float TurnPerTimeStep = TurnSpeedPerSec/60f;
        private readonly RevoluteJoint _frontLeftJoint;
        private readonly RevoluteJoint _frontRightJoint;
        private readonly IList<Tire> _frontTires;
        private readonly IList<Tire> _rearTires;

        public Car(World world, Sprite tireSprite)
        {
            _frontTires = new List<Tire>();
            _rearTires = new List<Tire>();

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
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.Texture.Width),
                    ConvertUnits.ToSimUnits(tireSprite.Texture.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            var joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(-0.5f, 2.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);

            _frontTires.Add(tire);
            _frontRightJoint = joint;

            tire = new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.Texture.Width),
                    ConvertUnits.ToSimUnits(tireSprite.Texture.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(0.5f, 2.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);

            _frontTires.Add(tire);
            _frontLeftJoint = joint;

            tire = new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.Texture.Width),
                    ConvertUnits.ToSimUnits(tireSprite.Texture.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(0.5f, 0.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);

            _rearTires.Add(tire);

            tire = new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.Texture.Width),
                    ConvertUnits.ToSimUnits(tireSprite.Texture.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(-0.5f, 0.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);

            _rearTires.Add(tire);
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

            foreach (var frontTire in _frontTires)
                frontTire.Update(gameTime);
            foreach (var rearTire in _rearTires)
                rearTire.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var frontTire in _frontTires)
                frontTire.Draw(spriteBatch);
            foreach (var rearTire in _rearTires)
                rearTire.Draw(spriteBatch);
        }
    }
}
