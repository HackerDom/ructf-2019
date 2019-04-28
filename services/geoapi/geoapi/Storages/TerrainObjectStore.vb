Imports System
Imports System.Collections.Generic
Imports geoapi.Models
Imports geoapi.Utils
Imports MongoDB.Driver

Namespace Storages
    Public Class TerrainObjectStore
        Implements ITerrainObjectStore

        Private ReadOnly terrainObjects As IMongoCollection(Of TerrainObject)
        Private ReadOnly settings As ISettings

        Public Sub New(ByVal settings As ISettings)
            Me.settings = settings
            Dim client = New MongoClient(settings.MongoDBConnectionString)
            Dim database = client.GetDatabase(settings.MongoDBName)
            terrainObjects = database.GetCollection(Of TerrainObject)(settings.TObjectsCollectionName)
            terrainObjects.Indexes.DropAll()
            terrainObjects.Indexes.CreateOneAsync(Builders(Of TerrainObject).IndexKeys.Ascending(Function(__) __.IndexKey)).GetAwaiter().GetResult()
            terrainObjects.Indexes.CreateOne(Builders(Of TerrainObject).IndexKeys.Ascending("expireAt"), New CreateIndexOptions With {
                                                .ExpireAfter = TimeSpan.FromSeconds(30)
                                                })
        End Sub

        Public Sub ITerrainObjectStore_UploadTerrainObject(terrainObject As TerrainObject) Implements ITerrainObjectStore.UploadTerrainObject
            terrainObjects.InsertOne(terrainObject)
        End Sub


        Public Function ITerrainObjectStore_GetTerrainObject(key As String) As TerrainObject Implements ITerrainObjectStore.GetTerrainObject
            Return terrainObjects.Find(Function(tobjcet) tobjcet.IndexKey.Equals(key)).FirstOrDefault()
        End Function

        Public Function ITerrainObjectStore_GetTerrainObjects(agentToken As String, skip As Integer, take As Integer) As IEnumerable(Of TerrainObject) Implements ITerrainObjectStore.GetTerrainObjects
            Return terrainObjects.Find(Function(tObject) tObject.IndexKey.StartsWith(agentToken)).Skip(skip).Limit(Math.Max(take - skip, settings.SearchLimit)).ToList()
        End Function
    End Class
End Namespace
