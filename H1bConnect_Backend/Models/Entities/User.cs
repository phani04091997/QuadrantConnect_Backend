using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace H1bConnect_Backend.Models.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // MongoDB ObjectId
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Store the hashed password
    }
}
