Imports System
Imports MongoDB.Bson
Imports MongoDB.Bson.Serialization.Attributes
Imports Newtonsoft.Json

Namespace Models
    Public Class AgentInfo
        <JsonIgnore>
        Public Property Id As ObjectId
        Public Property AgentToken As String
        Public Property AgentName As String
        <JsonIgnore>
        <BsonElement("expireAt")>
        Public ExpireAt As DateTime

        Public Sub New(ByVal agentToken As String, ByVal agentName As String, ByVal expireAt As DateTime)
            Me.AgentToken = agentToken
            Me.AgentName = agentName
            Me.ExpireAt = expireAt
        End Sub
    End Class
End Namespace
