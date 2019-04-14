using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace indexReact.db
{
    public interface IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        string Id { get; set; }
    }
}