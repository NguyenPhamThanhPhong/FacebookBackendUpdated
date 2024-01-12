using socialmediaAPI.Models.Embeded.Post;
using socialmediaAPI.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace socialmediaAPI.RequestsResponses.Requests
{
#pragma warning disable CS8618
    public class CreatePostRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<IFormFile>? Files { get; set; }
        [Required]
        public OwnerRepresentation Owner { get; set; }
        public string? SharedPostId { get; set; }

        public Post ConvertToPost()
        {
            return new Post()
            {
                Content = Content,
                Owner= Owner,
                SharedPost = ""
            };
        }
    }
}
