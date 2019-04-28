Imports System

Namespace Utils
    Public Interface ISettings
        Property ParallelismDegree As Integer
        Property Port As Integer
        Property MongoDBConnectionString As String
        Property AgentsCollectionName As String
        Property MongoDBName As String
        Property TObjectsCollectionName As String
        Property MaxContentLength As Integer
        Property AgentIdSize As Integer
        Property ObjectIdSize As Integer
        Property SearchLimit As Integer
        Property TTL As TimeSpan
    End Interface
End Namespace
