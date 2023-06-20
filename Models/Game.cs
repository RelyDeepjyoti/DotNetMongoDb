using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mongodb_dotnet_example.Models
{
    public class Game
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] 
        public string Id { get; set; }// = ObjectId.GenerateNewId().ToString();

        [BsonElement("Name")]
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Category { get; set; }
    }
    public class GameInput
    {
        [BsonElement("Name")]
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Category { get; set; }
    }
}