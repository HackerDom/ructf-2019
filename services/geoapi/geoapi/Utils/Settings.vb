Imports System

Namespace Utils
Public Class Settings
    Implements ISettings

    Public Property ParallelismDegree As Integer = 500 Implements ISettings.ParallelismDegree
    Public Property Port As Integer  = 9007 Implements ISettings.Port
    Public Property MongoDBConnectionString As String = "mongodb://localhost:27017" Implements ISettings.MongoDBConnectionString
    Public Property AgentsCollectionName As String = "AgentsCollection" Implements ISettings.AgentsCollectionName
    Public Property MongoDBName As String = "AgentsDB"  Implements ISettings.MongoDBName
    Public Property TObjectsCollectionName As String = "TObjecstCollection" Implements ISettings.TObjectsCollectionName
    Public Property MaxContentLength As Integer Implements ISettings.MaxContentLength
    Public Property AgentIdSize As Integer = 12 Implements ISettings.AgentIdSize
    Public Property ObjectIdSize As Integer = 12 Implements ISettings.ObjectIdSize
    Public Property SearchLimit As Integer = 100 Implements ISettings.SearchLimit
    Public Property TTL As TimeSpan = New TimeSpan(1, 30, 0) Implements ISettings.TTL
End Class
End Namespace