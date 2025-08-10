// src/Util/Epsg2230Transformer.cs
namespace CityTimelineMod.Util
{
    internal sealed class Epsg2230Transformer
    {
        private readonly double _offsetFeetX;
        private readonly double _offsetFeetY;
        private readonly double _metersPerFoot;

        public Epsg2230Transformer(double offsetFeetX, double offsetFeetY, double metersPerFoot)
        {
            _offsetFeetX = offsetFeetX;
            _offsetFeetY = offsetFeetY;
            _metersPerFoot = metersPerFoot;
        }

        // Convenience overload — build transformer directly from a BBox
        public Epsg2230Transformer(BBox bbox, double metersPerFoot)
            : this(bbox.min.x, bbox.min.y, metersPerFoot)
        {
        }

        public V2 ToWorld(V2 feet2230)
        {
            var x = (feet2230.x - _offsetFeetX) * _metersPerFoot;
            var y = (feet2230.y - _offsetFeetY) * _metersPerFoot;
            return new V2(x, y);
        }
    }
}
