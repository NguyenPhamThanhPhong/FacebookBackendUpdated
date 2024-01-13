using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using socialmediaAPI.Models.Embeded.User;
using System.Linq.Expressions;

#pragma warning disable CS8618

namespace socialmediaAPI.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; }
        public bool IsMailConfirmed { get; set; }
        public AuthenticationInformation AuthenticationInfo { get; set; }
        public PersonalInformation PersonalInfo { get; set; }
        public List<string> FriendWaitIds { get; set; }
        public List<string> FriendRequestIds { get; set; }
        public List<string> FriendIds { get; set; }
        public List<string> BlockedIds { get; set; }
        public List<string> PostIds { get; set; }
        public List<string> ConversationIds { get; set; }
        public List<Notification> Notifications { get; set; }
        public VerificationTicket EmailVerification { get; set; }
        public User()
        {
            ID = string.Empty;
            FriendWaitIds= new List<string>();
            FriendRequestIds = new List<string>();
            FriendIds = new List<string>();
            BlockedIds = new List<string>();
            PostIds = new List<string>();
            Notifications = new List<Notification>();
            ConversationIds = new List<string>();
        }
        public static string GetFieldName<T>(Expression<Func<User, T>> expression)
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
