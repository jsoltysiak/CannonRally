using System;
using System.Linq;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace CannonRally
{
    public class PathFollowerCarBehavior : ICarBehavior
    {
        private readonly Path _path;
        private int currentNodeIndex = 0;
        private Vector2 _currentTarget;

        public PathFollowerCarBehavior(Car car, Path path)
        {
            _path = path;
            Car = car;

            _currentTarget = _path.GetNodes()[currentNodeIndex];
        }

        public Car Car { get; }

        public float GetDesiredWheelAngle()
        {

            if (Vector2.Distance(Car.Body.Position, _currentTarget) < 2f)
            {
                if (++currentNodeIndex >= _path.GetNodes().Length)
                {
                    currentNodeIndex = 0;
                }
                _currentTarget = _path.GetNodes()[currentNodeIndex];
            }

            float desiredAngle = Seek(_currentTarget);

            return desiredAngle;
        }

        private float Seek(Vector2 target)
        {
            var currentForwardNormal = Car.Body.GetWorldVector(new Vector2(0, 1));
            var forwardVelocity = Vector2.Dot(currentForwardNormal, Car.Body.LinearVelocity) * currentForwardNormal;

            var desiredDirection = target - Car.Body.Position;

            var desiredAngle = (float)
                Math.Atan2(MathUtils.Cross(desiredDirection, currentForwardNormal),
                           MathUtils.Dot(new Vector3(desiredDirection, 0), new Vector3(currentForwardNormal, 0)));
            return desiredAngle;
        }

        public float GetDesiredSpeed()
        {
            var desiredSpeed = 0.0f;
            var target = new Vector2(64, 0);

            desiredSpeed = Car.MaxForwardSpeed;

            if (Vector2.Distance(target, Car.Body.Position) < 10)
            {
                desiredSpeed = Car.MaxForwardSpeed * 0.75f;
            }
            return desiredSpeed;
        }
    }
}