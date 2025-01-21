using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace H1bConnect_Backend.Models.Entities
{
    public class Counter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string CollectionName { get; set; }
        public int SequenceValue { get; set; }
    }
}
