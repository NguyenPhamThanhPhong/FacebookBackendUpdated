using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using socialmediaAPI.Models.Entities;

#pragma warning disable CS8618
namespace socialmediaAPI.Configs
{
    public class DatabaseConfigs
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UserCollectionName { get; set; }
        public string MessageCollectionName { get; set; }
        public string ConversationCollectionName { get; set; }
        public string PostCollectionName { get; set; }
        public string CommentCollectionName { get; set; }

        public IMongoClient? MongoClient { get; private set; }
        public IMongoDatabase? MongoDtb { get; private set; }

        public IMongoCollection<User> UserCollection { get; set; }
        public IMongoCollection<Message> MessageCollection { get; set; }
        public IMongoCollection<Conversation> ConversationCollection { get; set; }
        public IMongoCollection<Post> PostCollection { get; set; }
        public IMongoCollection<Comment> CommentCollection { get; set; }

        public void SetupDatabase()
        {
            InstantiateCollections();
            CreateUniqueIndex();
        }

        private void InstantiateCollections()
        {
            MongoClient = new MongoClient(ConnectionString);
            MongoDtb = MongoClient.GetDatabase(DatabaseName);


            UserCollection = MongoDtb.GetCollection<User>(UserCollectionName);
            MessageCollection = MongoDtb.GetCollection<Message>(MessageCollectionName);
            ConversationCollection = MongoDtb.GetCollection<Conversation>(ConversationCollectionName);
            PostCollection = MongoDtb.GetCollection<Post>(PostCollectionName);
            CommentCollection = MongoDtb.GetCollection<Comment>(CommentCollectionName);
        }
        private void CreateUniqueIndex()
        {
            //var indexKeysDefinition = Builders<User>.IndexKeys.Ascending(User.GetFieldName(u=>u.AuthenticationInfo.Username));
            //var indexOptions = new CreateIndexOptions { Unique = true };

            //UserCollection.Indexes.CreateOne(new CreateIndexModel<User>(indexKeysDefinition, indexOptions));
        }
    }
}
