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
            Body.ApplyLinearImpulse(new Vector2(20.0f, 20.0f));
        }

        public Vector2 Position { get; set; }
        public Body Body { get; set; }
        public Sprite Sprite { get; set; }

        public float DragForceMultiplier { get; set; } = 4f;

        public void Update(GameTime gameTime)
        {
            HandleInput();
            UpdateFriction();
        }

        private void HandleInput()
        {
            const float maxForwardSpeed = 100f;
            const float maxBackwardSpeed = -20f;
            const float maxDriveForce = -20f;

            float desiredSpeed = 0.0f;
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.W))
            {
                desiredSpeed = maxForwardSpeed;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                desiredSpeed = maxBackwardSpeed;
            }

            var currentForwardNormal = Body.GetWorldVector(new Vector2(0, 1));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite.Texture, ConvertUnits.ToDisplayUnits(Body.Position), null, Color.White,
                Body.Rotation, Sprite.Origin, 1f, SpriteEffects.None, 0f);
        }

        private void UpdateFriction()
        {
            var impulse = Body.Mass*-GetLateralVelocity();
            Body.ApplyLinearImpulse(impulse, Body.WorldCenter);
            Body.ApplyAngularImpulse(0.1f * Body.Inertia * -Body.AngularVelocity);

            Vector2 currentForwardNormal = GetForwardVelocity();
            if (Math.Abs(currentForwardNormal.Length()) > float.Epsilon) currentForwardNormal.Normalize();
            Vector2 dragForceMagniture = currentForwardNormal*(-DragForceMultiplier);
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