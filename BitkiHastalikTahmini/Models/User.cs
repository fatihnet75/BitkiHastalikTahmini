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

        [Required(ErrorMessage = "Ad alaný zorunludur")]
        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alaný zorunludur")]
        [BsonElement("LastName")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-posta alaný zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [BsonElement("Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Þifre alaný zorunludur")]
        [MinLength(6, ErrorMessage = "Þifre en az 6 karakter olmalýdýr")]
        [BsonElement("Password")]
        public string Password { get; set; }
    }
}   