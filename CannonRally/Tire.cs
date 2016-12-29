using System;
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

            Body.BodyType = BodyType.Dynamic;
        }

        public Vector2 Position { get; set; }
        public Body Body { get; set; }
        public Sprite Sprite { get; set; }

        public float DragForceMultiplier { get; set; } = 2f;

        public void Update(GameTime gameTime)
        {
            UpdateFriction();
            HandleInput();
        }

        private void HandleInput()
        {
            UpdateDrive();

            UpdateTurn();
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
            const float maxLateralImpulse = 0.06f;
            var impulse = Body.Mass*-GetLateralVelocity();
            if (impulse.Length() > maxLateralImpulse)
                impulse *= maxLateralImpulse/impulse.Length();
            Body.ApplyLinearImpulse(impulse, Body.WorldCenter);
            Body.ApplyAngularImpulse(0.1f*Body.Inertia*-Body.AngularVelocity);

            var currentForwardNormal = GetForwardVelocity();
            if (Math.Abs(currentForwardNormal.Length()) > 1) currentForwardNormal.Normalize();
            var dragForceMagniture = currentForwardNormal*-DragForceMultiplier;
            Body.ApplyForce(dragForceMagniture, Body.WorldCenter);
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