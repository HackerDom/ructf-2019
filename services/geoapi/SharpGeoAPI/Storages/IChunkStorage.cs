using System.Threading.Tasks;

namespace SharpGeoAPI.Storages
{
    interface IChunkStorage
    {
        Task SaveChunk();
    }
}