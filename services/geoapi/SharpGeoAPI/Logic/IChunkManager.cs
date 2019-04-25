using SharpGeoAPI.Models.Geo;
using SharpGeoAPI.Storages;

namespace SharpGeoAPI.Logic
{
    public interface IChunkManager
    {
        Chunk GetChunk(int chunkGroup, int chunkIndex);
    }
}