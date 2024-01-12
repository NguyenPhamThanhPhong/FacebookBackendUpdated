namespace socialmediaAPI.RequestsResponses.Requests
{
    public class ConversationCreateRequest
    {
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string>? AdminIDs { get; set; }
        public List<string>? ParticipantIds { get; set; }
        public bool IsGroup { get; set; }
        public IFormFile? File { get; set; }
    }
}
