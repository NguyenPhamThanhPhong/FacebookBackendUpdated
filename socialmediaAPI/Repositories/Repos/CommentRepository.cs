using MongoDB.Driver;
using socialmediaAPI.Configs;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.RequestsResponses.Requests;

namespace socialmediaAPI.Repositories.Repos
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IMongoCollection<Post> _postCollection;
        private readonly IMongoCollection<Comment> _commentCollection;
        public CommentRepository(DatabaseConfigs configuration)
        {
            _postCollection = configuration.PostCollection;
            _commentCollection = configuration.CommentCollection;
        }

        public async Task Create(Comment comment)
        {
            await _commentCollection.InsertOneAsync(comment);
            var filterPost = Builders<Post>.Filter.Eq(c => c.Id, comment.PostId);
            var updatePost =
                Builders<Post>.Update.Push(u => u.CommentIds, comment.Id);
            await _postCollection.UpdateOneAsync(filterPost, updatePost);
            return;
        }

        public async Task<Comment> Delete(string id)
        {
            var result = await _commentCollection.FindOneAndDeleteAsync(s => s.Id == id);
            var filterPost = Builders<Post>.Filter.Eq(s=>s.Id, result.PostId);
            var updatePost = Builders<Post>.Update.Pull(s => s.CommentIds, result.Id);
            await _postCollection.UpdateOneAsync(filterPost, updatePost);
            return result;
        }

        //public Task<Comment> GetById(string id)
        //{
        //    var filter = Builders<Comment>.Filter.Eq(p => p.Id, id);
        //    return _commentCollection.Find(filter).FirstOrDefaultAsync();
        //}

        public async Task<IEnumerable<Comment>> GetfromIds(IEnumerable<string> ids, int skip)
        {
            var filter = Builders<Comment>.Filter.In(s => s.Id, ids);
            var sort = Builders<Comment>.Sort.Descending(s => s.CommentTime);
            return await _commentCollection.Find(filter).Sort(sort).Limit(skip).ToListAsync();
        }

        public async Task UpdatebyParameters(string id, List<UpdateParameter> parameters)
        {
            var filter = Builders<Comment>.Filter.Eq(p => p.Id, id);
            var updateBuilder = Builders<Comment>.Update;
            List<UpdateDefinition<Comment>> subUpdates = new List<UpdateDefinition<Comment>>();
            foreach (var parameter in parameters)
            {
                subUpdates.Add(GetUpdatefromParameter(parameter));
            }
            var combinedUpdate = updateBuilder.Combine(subUpdates);
            await _commentCollection.UpdateOneAsync(filter, combinedUpdate);
        }

        public async Task UpdateContent(string id, string content)
        {
            var filter = Builders<Comment>.Filter.Eq(p => p.Id, id);
            var update = Builders<Comment>.Update.Set(c => c.Content, content);
            await _commentCollection.UpdateOneAsync(filter, update);
        }

        private UpdateDefinition<Comment> GetUpdatefromParameter(UpdateParameter parameter)
        {
            switch (parameter.updateAction)
            {
                case UpdateAction.set:
                    return Builders<Comment>.Update.Set<object>(parameter.FieldName, parameter.Value ?? "");
                case UpdateAction.push:
                    return Builders<Comment>.Update.Push<object>(parameter.FieldName, parameter.Value ?? "");
                case UpdateAction.pull:
                    return Builders<Comment>.Update.Pull<object>(parameter.FieldName, parameter.Value ?? "");
            }
            return Builders<Comment>.Update.Set<object>(parameter.FieldName, parameter.Value??"");

        }
    }
}
