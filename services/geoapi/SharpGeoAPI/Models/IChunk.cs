namespace SharpGeoAPI
{
    public interface IChunk
    {
        void Set(int x, int y, CellType cell);
        CellType Get(int x, int y);
    }
}