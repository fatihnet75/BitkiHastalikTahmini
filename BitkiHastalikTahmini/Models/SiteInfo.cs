using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class SiteInfo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("Title")]
    public string Title { get; set; } // Site ba�l���

    [BsonElement("Content")]
    public string Content { get; set; } // Hakk�nda yaz�s� vb.
}
