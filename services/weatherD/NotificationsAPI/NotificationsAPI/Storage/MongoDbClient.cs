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
	    private readonly string connectionString;
	    private const string SourceDataDbName = "test";
	    private const string MessagesDataDbName = "messages";
		//private string ConnectionString = "mongodb://localhost:27017";

		private readonly IMongoDatabase authorizesUsersDatabase;

		public MongoDbClient(string connectionString)
        {
	        this.connectionString = connectionString;
	        var client = new MongoClient(connectionString);
			authorizesUsersDatabase = client.GetDatabase(SourceDataDbName);
		}

        public async Task InsertUser(string source, string token, string password, bool isPublic)
        {
            var collection = authorizesUsersDatabase.GetCollection<SourceShortedData>(SourceDataDbName);
	        
			var newData = new SourceShortedData(source, token, password, isPublic);
            await collection.InsertOneAsync(newData);
        }

        public Task<List<SourceShortedData>> GetAllUsers()
        {
            var collection = authorizesUsersDatabase.GetCollection<SourceShortedData>(SourceDataDbName);
            return collection.Find(_ => true).ToListAsync();
        }

	    public async Task InsertMessage(string source, Message message)
	    {
		    var collection = authorizesUsersDatabase.GetCollection<StoredMessage>(MessagesDataDbName);
		    collection.Indexes.CreateOne(Builders<StoredMessage>.IndexKeys.Ascending("expireAt"), new CreateIndexOptions { ExpireAfter = new TimeSpan(0, 0, 10) });
			var newData = new StoredMessage(source, message.Content, message.ExpireAt);
		    await collection.InsertOneAsync(newData);
	    }

	    public Task<List<StoredMessage>> GetAllMessages()
	    {
		    var collection = authorizesUsersDatabase.GetCollection<StoredMessage>(MessagesDataDbName);
		    return collection.Find(_ => true).ToListAsync();
		}
	}
}
