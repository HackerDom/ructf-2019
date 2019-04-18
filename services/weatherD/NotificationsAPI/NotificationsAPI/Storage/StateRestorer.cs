using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationsApi.Documens;

namespace NotificationsApi.Storage
{
    internal class StateRestorer
    {
        private readonly MongoDbClient client;
        public StateRestorer(MongoDbClient client)
        {
            this.client = client;
        }

        public async Task<(Authorizer, SourceStorage)> Restore()
        {
	        var sourcesInfo = await client.GetAllUsers();
	        var authorizerData = new Dictionary<string, (string token, string password)>();
	        var sourceStorage = new SourceStorage();
			foreach(var data in sourcesInfo)
	        {
				if(!authorizerData.ContainsKey(data.Source))
				{
					authorizerData.Add(data.Source, (data.Token, data.Password));
					sourceStorage.Add(data.Source);
				}
	        }

	        var messageInfo = await client.GetAllMessages();
	        var prouppingMessages = messageInfo.GroupBy(x => x.SourceName);
	        foreach(var messages in prouppingMessages)
	        {
		       if(!sourceStorage.TryGetInfo(messages.Key, out var sourceInfo))
				   continue;

				foreach(var message in messages.OrderBy(x => x.expireAt))
		        {
					if(message.expireAt > DateTime.UtcNow)
						sourceInfo.AddMessage(new Message(message.Content, message.expireAt));
		        }
	        }

	        var authorizer = new Authorizer(authorizerData);
	        return (authorizer, sourceStorage);
        }

    }
}
