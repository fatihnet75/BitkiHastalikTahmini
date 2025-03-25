using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class PlantDisease
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("PlantImageId")]
    public string PlantImageId { get; set; } // Hangi resme ait

    [BsonElement("DiseaseName")]
    public string DiseaseName { get; set; } // Hastal�k ad�

    [BsonElement("DetectedAt")]
    public DateTime DetectedAt { get; set; } // Tespit tarihi
}
