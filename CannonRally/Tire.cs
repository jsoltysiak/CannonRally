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
        private const float DragForceMultiplier = 2f;
        private const float MaxForwardSpeed = 5f;
        private const float MaxBackwardSpeed = -5f;
        private const float MaxDriveForce = 5f;
        private const float MaxLateralImpulse = 0.1f;

        private readonly IList<GroundAreaUserData> _groundAreas;
        private float _traction;

        public Tire(Body body, Sprite sprite)
        {
            Body = body;
            Body.BodyType = BodyType.Dynamic;
            Body.UserData = this;
            Sprite = sprite;

            _groundAreas = new List<GroundAreaUserData>();
            _traction = 1.0f;
        }

        public Vector2 Position { get; set; }
        public Body Body { get; set; }
        public Sprite Sprite { get; set; }

        public void Update(GameTime gameTime)
        {
            UpdateFriction();
            UpdateDrive();
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
                    if (groundArea.FrictionModifier > _traction)
                        _traction = groundArea.FrictionModifier;
            }
        }

        private void UpdateDrive()
        {
            var desiredSpeed = 0.0f;
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.W))
                desiredSpeed = MaxForwardSpeed;
            else if (keyboardState.IsKeyDown(Keys.S))
                desiredSpeed = MaxBackwardSpeed;
            else return;

            var currentForwardNormal = Body.GetWorldVector(new Vector2(0, 1));
            var currentSpeed = Vector2.Dot(GetForwardVelocity(), currentForwardNormal);

            float force = 0;
            if (desiredSpeed > currentSpeed)
                force = MaxDriveForce;
            else if (desiredSpeed < currentSpeed)
                force = -MaxDriveForce;
            else
                return;

            Body.ApplyForce(force*currentForwardNormal, Body.WorldCenter);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite.Texture, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White,
                Body.Rotation, Sprite.Origin, 1f, SpriteEffects.None, 0f);
        }

        private void UpdateFriction()
        {
            var impulse = Body.Mass*-GetLateralVelocity();
            if (impulse.Length() > MaxLateralImpulse)
                impulse *= MaxLateralImpulse/impulse.Length();
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