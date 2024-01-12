namespace socialmediaAPI.RequestsResponses.Requests
{
    public class ConversationCreateRequestFriend
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public ConversationCreateRequestFriend()
        {
            Name = string.Empty;
            AvatarUrl = string.Empty;
        }
    }
}
