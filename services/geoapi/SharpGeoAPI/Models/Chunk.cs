namespace SharpGeoAPI.Models
{
    public class Chunk : IChunk
    {
        public Chunk(int width, int height)
        {
            this.width = width;
            this.height = height;
            Cells = new byte[width * height];
        }

        private readonly int width;
        private readonly int height;

        public byte[] Cells;

        public void Set(int x, int y, CellType cell)
        {
            Cells[CoordsToIndex(x, y)] = (byte) cell;
        }

        public CellType Get(int x, int y)
        {
            return (CellType)Cells[CoordsToIndex(x, y)];
        }

        private int CoordsToIndex(int x, int y)
        {
            return y * width + x;
        }
    }

    public interface IAgentsManager
    {
        void HandleMoveAction(Agent agent, Vector2 position);
        void HandleTerraformAction(Agent agent, Vector2 position, CellType cell);
    }
}