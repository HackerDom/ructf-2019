using System;
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
			
	        var authorizer = new Authorizer();
			
	        var sourceStorage = new SourceStorage();

	        var sourcesInfo = await client.GetAllUsers();
			foreach(var data in sourcesInfo)
	        {
				if(data.IsPublic)
					authorizer.RegisterPublic(data.Source, data.Password);
				else
					authorizer.RegisterPrivate(data.Source, data.Token, data.Password);
				sourceStorage.Add(data.Source);
			}

	        var messageInfo = await client.GetAllMessages();
	        var prouppingMessages = messageInfo.GroupBy(x => x.SourceName);
	        foreach(var messages in prouppingMessages)
	        {
		       if(!sourceStorage.TryGetInfo(messages.Key, out var sourceInfo))
				   continue;

				foreach(var message in messages.OrderBy(x => x.ExpireAt))
		        {
					if(message.ExpireAt > DateTime.UtcNow)
						sourceInfo.AddMessage(new Message(message.Content, message.ExpireAt));
		        }
	        }


	        return (authorizer, sourceStorage);
        }

    }
}
