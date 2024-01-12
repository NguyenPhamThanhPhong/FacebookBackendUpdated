using System.ComponentModel.DataAnnotations;

namespace socialmediaAPI.RequestsResponses.Requests
{
    public class MessageCreateRequest
    {
        [Required]
        public string ConversationId { get; set; }
        [Required]
        public string SenderID { get; set; }
        public string Content { get; set; }
        public string? ReplyToId { get; set; }
        public List<IFormFile>? Files { get; set; }
        public MessageCreateRequest() {
            SenderID = string.Empty;
            Content = string.Empty;
            ConversationId = string.Empty;
        }

    }
}
