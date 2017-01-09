using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace CannonRally
{
    public class ManualCarBehavior : ICarBehavior
    {
        public ManualCarBehavior(Car car)
        {
            Car = car;
        }

        public Car Car { get; }

        public float GetDesiredWheelAngle()
        {
            float desiredAngle = 0;
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                desiredAngle = -Car.LockAngle;
            else if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                desiredAngle = Car.LockAngle;
            return desiredAngle;
        }

        public float GetDesiredSpeed()
        {
            var desiredSpeed = 0.0f;
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                desiredSpeed = Car.MaxForwardSpeed;
            else if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                desiredSpeed = Car.MaxBackwardSpeed;

            return desiredSpeed;
        }
    }
}