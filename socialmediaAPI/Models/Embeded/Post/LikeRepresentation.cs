using socialmediaAPI.Models.Enums;

namespace socialmediaAPI.Models.Embeded.Post
{
#pragma warning disable CS8618

    public class LikeRepresentation
    {
        public string UserId { get; set; }
        public Emoji Emo { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return UserId == ((LikeRepresentation)obj).UserId;
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode();
        }
    }
}
