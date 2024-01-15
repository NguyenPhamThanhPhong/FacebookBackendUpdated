using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using socialmediaAPI.Models.Entities;

namespace socialmediaAPI.RequestsResponses.Requests
{
    public class CreateCommentRequest
    {
        public string? PostId { get; set; }
        public string? ParentId { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; }
        public List<IFormFile>? Files { get; set; }
        public CreateCommentRequest()
        {
            Content = string.Empty;
        }
        public Comment converToComment()
        {
            return new Comment
            {
                ParentId = ParentId,
                UserId = UserId,
                Content = Content,
                PostId = PostId
            };
        }
    }
}
