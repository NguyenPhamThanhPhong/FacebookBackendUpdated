using MongoDB.Bson.Serialization.Attributes;

namespace socialmediaAPI.Models.Embeded.User
{
    [BsonIgnoreExtraElements]
    public class PersonalInformation
    {
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        public DateTime? DateofBirth { get; set; }
        public string LiveAt { get; set; }
        public string StudyAt { get; set; }
        public string Address { get; set; }
        public string RelationShip { get; set; }
        public string Phone { get; set; }
        public string? Favorites { get; set; }
        public string? Biography { get; set; }
        public PersonalInformation()
        {
            Name = "";
            AvatarUrl = "";
            LiveAt = "";
            StudyAt = "";
            Address = "";
            RelationShip = "";
            Phone = "";
        }
    }
}
