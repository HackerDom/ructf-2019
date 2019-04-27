using System;
using MongoDB.Driver;
using NotificationsApi.Documens;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Configuration;

namespace NotificationsApi.Storage
{
    internal class MongoDbClient
    {
	    private const string SourceDataDbName = "test";
	    private const string MessagesDataDbName = "messages";

		private readonly IMongoDatabase authorizesUsersDatabase;
	    private IMongoCollection<SourceShortedData> sourecShortedDataCollection;
	    private IMongoCollection<StoredMessage> storedMessageCollection;
		public MongoDbClient(string connectionString, TimeSpan ttl)
        {
	        var client = new MongoClient(connectionString);
			authorizesUsersDatabase = client.GetDatabase(SourceDataDbName);
			sourecShortedDataCollection = authorizesUsersDatabase.GetCollection<SourceShortedData>(SourceDataDbName);
	        storedMessageCollection = authorizesUsersDatabase.GetCollection<StoredMessage>(MessagesDataDbName);
			
	        sourecShortedDataCollection.Indexes.CreateOne(Builders<SourceShortedData>.IndexKeys.Ascending("expireAt"), new CreateIndexOptions { ExpireAfter = ttl});
	        storedMessageCollection.Indexes.CreateOne(Builders<StoredMessage>.IndexKeys.Ascending("expireAt"), new CreateIndexOptions { ExpireAfter = ttl });

		}

		public async Task InsertUser(string source, string token, string password, bool isPublic)
        { 
			var newData = new SourceShortedData(source, token, password, isPublic);
            await sourecShortedDataCollection.InsertOneAsync(newData);
        }

        public Task<List<SourceShortedData>> GetAllUsers()
        {
	        return sourecShortedDataCollection.Find(_ => true).ToListAsync();
        }

	    public async Task InsertMessage(string source, Message message)
	    {
			var newData = new StoredMessage(source, message.Content, message.ExpireAt);
		    await storedMessageCollection.InsertOneAsync(newData);
	    }

	    public Task<List<StoredMessage>> GetAllMessages()
	    {
		    return storedMessageCollection.Find(_ => true).ToListAsync();
		}
	}
}
