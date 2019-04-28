Imports System
Imports geoapi.Models
Imports geoapi.Utils
Imports MongoDB.Driver

Namespace Storages
    Class AgentStorage
        Implements IAgentStorage

        Private ReadOnly agents As IMongoCollection(Of AgentInfo)

        Public Sub New(ByVal settings As ISettings)
            Dim client = New MongoClient(settings.MongoDBConnectionString)
            Dim database = client.GetDatabase(settings.MongoDBName)
            agents = database.GetCollection(Of AgentInfo)(settings.AgentsCollectionName)
            agents.Indexes.DropAll()
            agents.Indexes.CreateOneAsync(Builders(Of AgentInfo).IndexKeys.Ascending(Function(__) __.AgentToken)).GetAwaiter().GetResult()
            agents.Indexes.CreateOne(Builders(Of AgentInfo).IndexKeys.Ascending("expireAt"), New CreateIndexOptions With {
                                        .ExpireAfter = TimeSpan.FromSeconds(30)
                                        })
        End Sub

        Public Sub IAgentStorage_AddAgent(agentInfo As AgentInfo) Implements IAgentStorage.AddAgent
            agents.InsertOne(agentInfo)
        End Sub

        Public Function IAgentStorage_GetAgent(agentId As String) As AgentInfo Implements IAgentStorage.GetAgent
            Return agents.Find(Function(agent) agent.AgentToken.Equals(agentId)).FirstOrDefault()
        End Function

    End Class
End Namespace
