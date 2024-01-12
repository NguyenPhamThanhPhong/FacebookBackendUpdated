using MongoDB.Bson.Serialization.Attributes;
using System.Linq.Expressions;

#pragma warning disable CS8618
namespace socialmediaAPI.Models.Entities
{
    [BsonIgnoreExtraElements]

    public class Message
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string ConversationId { get; set; }
        public string SenderID { get; set; }
        public string? Content { get; set; }
        public DateTime? SendTime { get; set; }
        public string? ReplyToId { get; set; }
        public Dictionary<string,string?>? FileUrls { get; set; }

        [BsonIgnoreIfNull]
        public string? CallRoomUrl { get; set; }


        public Message()
        {
            Id = string.Empty;
            SendTime = DateTime.Now;
        }

        public static string GetFieldName<T>(Expression<Func<Message, T>> expression)
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
