namespace CannonRally.FixtureUserData
{
    public class GroundAreaUserData : FixtureUserData
    {
        public GroundAreaUserData(float frictionModifier, bool outOfCourse) : base(FixtureUserDataType.GroundArea)
        {
            FrictionModifier = frictionModifier;
            OutOfCourse = outOfCourse;
        }

        public float FrictionModifier { get; private set; }
        public bool OutOfCourse { get; private set; }
    }
}