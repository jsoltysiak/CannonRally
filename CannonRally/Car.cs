using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    class Car
    {
        public Body Body { get; private set; }
        private readonly IList<Tire> _frontTires;
        private readonly IList<Tire> _rearTires;
        private RevoluteJoint frontLeftJoint;
        private RevoluteJoint frontRightJoint;

        public Car(World world, Sprite tireSprite)
        {
            _frontTires = new List<Tire>();
            _rearTires = new List<Tire>();

            Vertices vertices = new Vertices();
            vertices.Add(new Vector2( .5f, 0));
            vertices.Add(new Vector2( .5f, 1f));
            vertices.Add(new Vector2( .5f, 2f));
            vertices.Add(new Vector2( .25f, 3));
            vertices.Add(new Vector2(-.25f, 3));
            vertices.Add(new Vector2(-.5f, 2f));
            vertices.Add(new Vector2(-.5f, 1f));
            vertices.Add(new Vector2(-.5f, 0));

            Body = BodyFactory.CreatePolygon(world, vertices, 1.0f);
            Body.BodyType = BodyType.Dynamic;

            Tire tire = new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.Texture.Width),
                    ConvertUnits.ToSimUnits(tireSprite.Texture.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            var joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(-0.5f, 2.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0, 0);

            _frontTires.Add(tire);
            frontRightJoint = joint;

            tire = new Tire(
                BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(tireSprite.Texture.Width),
                    ConvertUnits.ToSimUnits(tireSprite.Texture.Height), 0.1f, 0.1f, 0, 1f,
                    userData: new TireUserData()), tireSprite);

            joint = JointFactory.CreateRevoluteJoint(world, Body, tire.Body, new Vector2(0.5f, 2.5f), Vector2.Zero);
            joint.LimitEnabled = true;
            joint.SetLimits(0,0);

            _frontTires.Add(tire);
            frontLeftJoint = joint;

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

        public void Update(GameTime gameTime)
        {
            const float lockAngle = 40f*MathHelper.Pi/180f;
            const float turnSpeedPerSec = 320*MathHelper.Pi/180f;
            const float turnPerTimeStep = turnSpeedPerSec/60f;
            float desiredAngle = 0;

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.A))
                desiredAngle = -lockAngle;
            else if (keyboardState.IsKeyDown(Keys.D))
                desiredAngle = lockAngle;

            float angleNow = frontLeftJoint.JointAngle;
            float angleToTurn = desiredAngle - angleNow;
            angleToTurn = MathUtils.Clamp(angleToTurn, -turnPerTimeStep, turnPerTimeStep);
            float newAngle = angleNow + angleToTurn;
            frontLeftJoint.SetLimits(newAngle, newAngle);
            frontRightJoint.SetLimits(newAngle, newAngle);

            foreach (var frontTire in _frontTires)
            {
                frontTire.Update(gameTime);
            }
            foreach (var rearTire in _rearTires)
            {
                rearTire.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var frontTire in _frontTires)
            {
                frontTire.Draw(spriteBatch);
            }
            foreach (var rearTire in _rearTires)
            {
                rearTire.Draw(spriteBatch);
            }
        }
    }
}