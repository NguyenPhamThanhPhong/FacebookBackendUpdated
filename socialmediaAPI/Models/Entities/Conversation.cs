using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace socialmediaAPI.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }

        public List<string>? AdminIDs { get; set; }
        public List<string>? ParticipantIds { get; set; }
        public bool IsGroup { get; set; }
        public DateTime RecentTime { get; set; }
        public string RecentMessage { get; set; }
        public List<string> MessageIds { get; set; }

        public Conversation()
        {
            ID = string.Empty;
            AdminIDs = new List<string>();
            ParticipantIds = new List<string>();
            IsGroup = false;
            MessageIds = new List<string>();
            RecentMessage = string.Empty;
            RecentTime = DateTime.Now;
        }
    }
    public class MessageDisplay
    {
        public string? senderId { get; set; }
        public string? Content { get; set; }
        public DateTime RecentTime { get; set; }
    }
}
