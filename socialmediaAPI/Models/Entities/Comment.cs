using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace socialmediaAPI.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string PostId { get; set; }
        public string? ParentId { get; set; }
        public string? UserId { get; set; }
        public DateTime CommentTime { get; set; }
        public string Content { get; set; }
        public Dictionary<string,string?> FileUrls { get; set; }
        public bool isEdited { get; set; }
        public Comment()
        {
            Id = string.Empty;
            PostId = string.Empty;
            CommentTime = DateTime.UtcNow;
            Content = string.Empty;
            FileUrls = new Dictionary<string,string?>();
            isEdited = false;
        }
        public static string GetFieldName<T>(Expression<Func<Comment, T>> expression)
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
