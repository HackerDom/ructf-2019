using System.Drawing;

namespace SharpGeoAPI.Models
{
    public class Bound
    {
        public Bound(float xMin, float xMax, float yMin, float yMax)
        {
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
        }

        public float XMin { get; }
        public float XMax { get; }
        public float YMin { get; }
        public float YMax { get; }

        public bool Contain(Point point)
        {
            return XMin <= point.X && XMax >= point.X && YMin <= point.Y && YMax >= point.Y;
        }
    }
}