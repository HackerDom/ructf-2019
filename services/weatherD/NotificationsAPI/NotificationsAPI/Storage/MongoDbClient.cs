using MongoDB.Driver;
using NotificationsApi.Documens;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationsApi.Storage
{
    internal class MongoDbClient
    {
        private const string SourceDataDbName = "test";
	    private const string MessagesDataDbName = "messages";
		private const string ConnectionString = "mongodb://localhost:27017";

		private readonly IMongoDatabase authorizesUsersDatabase;


		public MongoDbClient()
        {
			var client = new MongoClient(ConnectionString);
			authorizesUsersDatabase = client.GetDatabase(SourceDataDbName);
		}

        public async Task InsertUser(string source, string token, string password)
        {
            var collection = authorizesUsersDatabase.GetCollection<SourceShortedData>(SourceDataDbName);
            var newData = new SourceShortedData(source, token, password);
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
