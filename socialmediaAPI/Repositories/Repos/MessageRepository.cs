using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using socialmediaAPI.Configs;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.RequestsResponses.Requests;
using System.Text.Json;

namespace socialmediaAPI.Repositories.Repos
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IMongoCollection<Message> _messageCollection;
        private readonly IMongoCollection<Conversation> _conversationCollection;

        public MessageRepository( DatabaseConfigs databaseConfigs)
        {
            _messageCollection = databaseConfigs.MessageCollection;
            _conversationCollection = databaseConfigs.ConversationCollection;
        }

        public async Task Create(Message message)
        {
            await _messageCollection.InsertOneAsync(message);
            var conversationFilter = Builders<Conversation>.Filter.Eq(c => c.ID, message.ConversationId);
            var converationUpdate = Builders<Conversation>.Update.Push(c => c.MessageIds, message.Id).Set(s => s.RecentMessage, message.Content ?? "");
            await _conversationCollection.UpdateOneAsync(conversationFilter, converationUpdate);

        }

        public async Task<Message> Delete(string id)
        {
            var deletedMessage = await _messageCollection.FindOneAndDeleteAsync(d=>d.Id== id);
            var conversationFilter = Builders<Conversation>.Filter.Eq(c => c.ID,deletedMessage.ConversationId);
            var converationUpdate = Builders<Conversation>.Update.Pull(c => c.MessageIds, id).Set(s => s.RecentMessage, "");
            await _conversationCollection.UpdateOneAsync(conversationFilter, converationUpdate);
            return deletedMessage;
        }

        public async Task<IEnumerable<Message>> GetbyIds(IEnumerable<string> ids,int skip)
        {
            var filter = Builders<Message>.Filter.In(m=>m.Id,ids);
            return await _messageCollection.Find(filter).Skip(skip).Limit(50).ToListAsync();
        }

        public Task UpdatebyParameters(string id, IEnumerable<UpdateParameter> parameters)
        {
            var filter = Builders<Message>.Filter.Eq(p => p.Id, id);
            var updateBuilder = Builders<Message>.Update;
            List<UpdateDefinition<Message>> subUpdates = new List<UpdateDefinition<Message>>();

            foreach (var parameter in parameters)
            {
                switch (parameter.updateAction)
                {
                    case UpdateAction.set:
                        subUpdates.Add(Builders<Message>.Update.Set(parameter.FieldName, parameter.Value));
                        continue;
                    case UpdateAction.push:
                        subUpdates.Add(Builders<Message>.Update.Push(parameter.FieldName, parameter.Value));
                        continue;
                    case UpdateAction.pull:
                        subUpdates.Add(Builders<Message>.Update.Pull(parameter.FieldName, parameter.Value));
                        continue;
                }
            }
            var combinedUpdate = updateBuilder.Combine(subUpdates);
            return _messageCollection.UpdateOneAsync(filter, combinedUpdate);
        }

        public async Task UpdateContent(string id, string content)
        {
            var filter = Builders<Message>.Filter.Eq(p => p.Id, id);
            var update = Builders<Message>.Update.Set(c => c.Content, content);
            var message = await _messageCollection.FindOneAndUpdateAsync(filter, update);
            var conversationFilter = Builders<Conversation>.Filter.Eq(c => c.ID, message.ConversationId);
            var converationUpdate = Builders<Conversation>.Update.Push(c => c.MessageIds, message.Id).Set(s => s.RecentMessage, message.Content ?? "");
            await _conversationCollection.UpdateOneAsync(conversationFilter, converationUpdate);
        }
    }
}
//public async Task<IEnumerable<Message>> GetbyFilterString(string filterString)
//{
//    var filterDocument = BsonSerializer.Deserialize<BsonDocument>(filterString);
//    var filter = new BsonDocumentFilterDefinition<Message>(filterDocument);
//    return await _messageCollection.Find(filter).ToListAsync();
//}