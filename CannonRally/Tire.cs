using System;
using System.Collections.Generic;
using CannonRally.FixtureUserData;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CannonRally
{
    public class Tire
    {
        public Tire(Body body, Sprite sprite)
        {
            Body = body;
            Sprite = sprite;

            _groundAreas = new List<GroundAreaUserData>();
            _traction = 1.0f;

            Body.BodyType = BodyType.Dynamic;
            body.UserData = this;
        }

        public Vector2 Position { get; set; }
        public Body Body { get; set; }
        public Sprite Sprite { get; set; }

        private readonly IList<GroundAreaUserData> _groundAreas;
        private float _traction;
        public float DragForceMultiplier { get; set; } = 2f;

        public void Update(GameTime gameTime)
        {
            UpdateFriction();
            UpdateDrive();
            UpdateTurn();
        }

        public void AddGroundArea(GroundAreaUserData ground)
        {
            _groundAreas.Add(ground);
            UpdateTraction();
        }

        public void RemoveGroundArea(GroundAreaUserData ground)
        {
            _groundAreas.Remove(ground);
            UpdateTraction();
        }

        private void UpdateTraction()
        {
            if (_groundAreas.Count == 0)
            {
                _traction = 1;
            }
            else
            {
                _traction = 0;
                foreach (var groundArea in _groundAreas)
                {
                    if (groundArea.FrictionModifier > _traction)
                    {
                        _traction = groundArea.FrictionModifier;
                    }
                }
            }
        }

        private void UpdateDrive()
        {
            const float maxForwardSpeed = 5f;
            const float maxBackwardSpeed = -5f;
            const float maxDriveForce = 5f;

            var desiredSpeed = 0.0f;
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.W))
                desiredSpeed = maxForwardSpeed;
            else if (keyboardState.IsKeyDown(Keys.S))
                desiredSpeed = maxBackwardSpeed;
            else return;

            var currentForwardNormal = Body.GetWorldVector(new Vector2(0, 1));
            var currentSpeed = Vector2.Dot(GetForwardVelocity(), currentForwardNormal);

            float force = 0;
            if (desiredSpeed > currentSpeed)
                force = maxDriveForce;
            else if (desiredSpeed < currentSpeed)
                force = -maxDriveForce;
            else
                return;

            Body.ApplyForce(force*currentForwardNormal, Body.WorldCenter);
        }

        private void UpdateTurn()
        {
            const float maxTorque = 0.3f;
            var desiredTorque = 0.0f;
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.A))
                desiredTorque = -maxTorque;
            else if (keyboardState.IsKeyDown(Keys.D))
                desiredTorque = maxTorque;

            Body.ApplyTorque(desiredTorque);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite.Texture, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White,
                Body.Rotation, Sprite.Origin, 1f, SpriteEffects.None, 0f);
        }

        private void UpdateFriction()
        {
            const float maxLateralImpulse = 0.1f;
            var impulse = Body.Mass*-GetLateralVelocity();
            if (impulse.Length() > maxLateralImpulse)
                impulse *= maxLateralImpulse/impulse.Length();
            Body.ApplyLinearImpulse(_traction*impulse, Body.WorldCenter);
            Body.ApplyAngularImpulse(_traction*0.1f*Body.Inertia*-Body.AngularVelocity);

            var currentForwardNormal = GetForwardVelocity();
            if (Math.Abs(currentForwardNormal.Length()) > 1) currentForwardNormal.Normalize();
            var dragForceMagniture = currentForwardNormal*-DragForceMultiplier;
            Body.ApplyForce(_traction*dragForceMagniture, Body.WorldCenter);
        }

        private Vector2 GetLateralVelocity()
        {
            var currentRightNormal = Body.GetWorldVector(new Vector2(1, 0));
            return Vector2.Dot(currentRightNormal, Body.LinearVelocity)*currentRightNormal;
        }

        private Vector2 GetForwardVelocity()
        {
            var currentForwardNormal = Body.GetWorldVector(new Vector2(0, 1));
            return Vector2.Dot(currentForwardNormal, Body.LinearVelocity)*currentForwardNormal;
        }
    }
}