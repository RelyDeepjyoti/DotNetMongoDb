using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

//public class Like
//{
//    [BsonId]
//    [BsonRepresentation(BsonType.ObjectId)]
//    public string Id { get; set; }
//    public string PostId { get; set; }
//    public string UserId { get; set; }
//    public DateTime Posted { get; set; }
//    public bool IsLike { get; set; }
//} 

public class Like
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string PostId { get; set; }
    public DateTime Posted { get; set; }
    public bool IsLike { get; set; }
}
