namespace CannonRally.FixtureUserData
{
    public class FixtureUserData
    {
        public FixtureUserData(FixtureUserDataType type)
        {
            Type = type;
        }

        public FixtureUserDataType Type { get; private set; }
    }

    public enum FixtureUserDataType
    {
        CarTire,
        GroundArea
    }
}