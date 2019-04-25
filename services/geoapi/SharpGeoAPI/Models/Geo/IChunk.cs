using System.Threading.Tasks;

namespace SharpGeoAPI.Models.Geo
{
    public interface IChunk
    {
        bool TrySet(Vector2 point, CellType cell);
        Task Update();
    }
}