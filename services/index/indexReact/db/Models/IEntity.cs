using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace indexReact.db.Models
{
    public interface IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        string Id { get; set; }
    }
}