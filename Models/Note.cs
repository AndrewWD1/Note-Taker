using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesApp.Models
{
    public class Note
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Text { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool Complete { get; set; }
    }
}