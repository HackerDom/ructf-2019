namespace SharpGeoAPI.Models.Geo
{
    public class Chunk : IChunk
    {
        public Chunk(byte[] cells)
        {
            this.Cells = cells;
        }

        public Chunk(int width, int height)
        {
            this.width = width;
            this.height = height;
            Cells = new byte[width * height];
        }

        public Chunk(int width, int height, byte[] cells)
        {
            this.width = width;
            this.height = height;
            this.Cells = cells;
        }

        private readonly int width;
        private readonly int height;

        public readonly byte[] Cells;

        public void Set(Vector2 point, CellType cell)
        {
            Cells[CoordsToIndex(point.X, point.Y)] = (byte) cell;
        }

        public CellType Get(Vector2 point)
        {
            return (CellType)Cells[CoordsToIndex(point.X, point.Y)];
        }

        private int CoordsToIndex(int x, int y)
        {
            return y * width + x;
        }
    }
}