using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class PlantImage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("UserId")]
    public string UserId { get; set; } // Kullanıcıya referans

    [BsonElement("ImageUrl")]
    public string ImageUrl { get; set; } // Resim URL olarak saklanıyor

   
}
