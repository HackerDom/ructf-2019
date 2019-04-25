namespace SharpGeoAPI.Models.Geo
{
    public class Tile
    {
        public Tile(Vector2 position, CellType cell)
        {
            this.position = position;
            Cell = cell;
        }

        public Vector2 position;
        public CellType Cell;
    }
}