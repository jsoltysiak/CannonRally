using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CannonRally
{
    public class PathFollowerCarBehavior : ICarBehavior
    {
        public PathFollowerCarBehavior(Car car)
        {
            Car = car;
        }

        public Car Car { get; }

        public float GetDesiredWheelAngle()
        {
            float desiredAngle = 0;
            desiredAngle = MathHelper.ToRadians(20);
            return desiredAngle;
        }

        public float GetDesiredSpeed()
        {
            var desiredSpeed = 0.0f;
            desiredSpeed = 25;

            return desiredSpeed;
        }
    }
}