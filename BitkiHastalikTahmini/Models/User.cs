using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
namespace BitkiHastalikTahmini.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // Generate ID automatically

        [BsonElement("UserId")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Ad alan� zorunludur")]
        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alan� zorunludur")]
        [BsonElement("LastName")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-posta alan� zorunludur")]
        [EmailAddress(ErrorMessage = "Ge�erli bir e-posta adresi giriniz")]
        [BsonElement("Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "�ifre alan� zorunludur")]
        [MinLength(6, ErrorMessage = "�ifre en az 6 karakter olmal�d�r")]
        [BsonElement("Password")]
        public string Password { get; set; }
    }
}   