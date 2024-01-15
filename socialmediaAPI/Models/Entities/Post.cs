using MongoDB.Bson.Serialization.Attributes;
using socialmediaAPI.Models.Embeded.Post;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace socialmediaAPI.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Post
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string Content { get; set; }
        public string? SharedPost { get; set; }
        public Dictionary<string, string?>? FileUrls { get; set; }
        public DateTime RecentTime { get; set; }
        public OwnerRepresentation Owner { get; set; }
        public List<LikeRepresentation> Likes { get; set; }
        public List<string> CommentIds { get; set; }
        public Post()
        {
            Id = string.Empty;
            Content = string.Empty;
            SharedPost = string.Empty;
            Owner = new OwnerRepresentation();
            RecentTime = DateTime.Now;
            Likes = new List<LikeRepresentation>();
            CommentIds = new List<string>();
        }
        public static string GetFieldName<T>(Expression<Func<Post, T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new ArgumentException("Invalid expression. Must be a property access expression.", nameof(expression));
            }

            var stack = new Stack<string>();

            while (memberExpression != null)
            {
                stack.Push(memberExpression.Member.Name);
                memberExpression = memberExpression.Expression as MemberExpression;
            }

            return string.Join(".", stack);
        }

    }
}
