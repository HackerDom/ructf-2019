using System.Collections.Generic;
using geoapi.Models;

namespace geoapi.Storages
{
    public interface ITerrainObjectStore
    {
        TerrainObject GetTerrainObject(string objectId);
        void UploadTerrainObject( TerrainObject terrainObject);
        IEnumerable<TerrainObject> GetTerrainObjects(string ownerIndex, int skip, int take);
    }
}