Imports System
Imports MongoDB.Bson
Imports MongoDB.Bson.Serialization.Attributes
Imports Newtonsoft.Json

Namespace Models
    Public Class TerrainObject
        <JsonIgnore>
        Public Property Id As ObjectId
        Public Property IndexKey As String
        Public Property Info As String
        Public Property Cells As CellTypes(,)
        <JsonIgnore>
        <BsonElement("expireAt")>
        Public ReadOnly Property ExpireAt As DateTime

        Public Sub New(ByVal agentKey As String, ByVal objectKey As String, ByVal expireAt As DateTime)
            Me.ExpireAt = expireAt
            Me.IndexKey = agentKey & objectKey
        End Sub
    End Class
End Namespace
