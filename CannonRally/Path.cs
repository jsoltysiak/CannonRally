using Microsoft.Xna.Framework;

namespace CannonRally
{
    public class Path
    {
        private readonly Vector2[] _nodes;

        public Path(Vector2[] nodes)
        {
            _nodes = nodes;
        }

        private Vector2[] GetNodes()
        {
            return _nodes;
        }
    }
}