namespace SharpGeoAPI.Models.Geo
{
    public interface IChunk
    {
        void Set(Vector2 point, CellType cell);
        CellType Get(Vector2 point);
    }
}