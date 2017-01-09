using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CannonRally
{
    public interface ICarBehavior
    {
        Car Car { get; }

        float GetDesiredWheelAngle();
        float GetDesiredSpeed();
    }
}
