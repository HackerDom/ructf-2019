using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace indexReact.db.Models
{
    public class User : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Login")]
        public string Login { get; set; }

        [BsonElement("PwdHash")]
        public string PwdHash { get; set; }
    }
}