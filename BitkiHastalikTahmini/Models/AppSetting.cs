using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BitkiHastalikTahmini.Models
{
    public class AppSetting
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("Key")]
        public string Key { get; set; }
        
        [BsonElement("Value")]
        public string Value { get; set; }
    }
} 