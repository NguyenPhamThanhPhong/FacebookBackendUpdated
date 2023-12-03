﻿using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using socialmediaAPI.Configs;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.RequestsResponses.Requests;

namespace socialmediaAPI.Repositories.Repos
{
    public class PostRepository : IPostRepository
    {
        private IMongoCollection<User> _userCollection;
        private IMongoCollection<Post> _postCollection;
        private IMongoCollection<CommentLog> _commentLogCollection;
        public PostRepository(DatabaseConfigs configuration)
        {
            _userCollection = configuration.UserCollection;
            _postCollection = configuration.PostCollection;
            _commentLogCollection = configuration.CommentLogCollection;
        }
        public async Task CreatePost(Post post)
        {
            await _postCollection.InsertOneAsync(post);
            var filterUser = Builders<User>.Filter.Eq(u => u.ID, post.Owner.UserId);
            var updateUser = Builders<User>.Update.Push(u => u.PostIds, post.Id);
            await _userCollection.UpdateOneAsync(filterUser, updateUser);
        }

        public async Task Delete(string id)
        {
            var deletedPost = await _postCollection.FindOneAndDeleteAsync(p => p.Id == id);

            var filterUser = Builders<User>.Filter.Eq(u => u.ID, deletedPost.Owner.UserId);
            var updateUser = Builders<User>.Update.Pull(u => u.PostIds, deletedPost.Id);
            await _userCollection.UpdateOneAsync(filterUser, updateUser);

            var filterCommentLog = Builders<CommentLog>.Filter.In(c => c.Id, deletedPost.CommentLogIds);
            await _commentLogCollection.DeleteManyAsync(filterCommentLog);
        }

        public Task<List<Post>> GetbyFilterString(string filterString)
        {
            var filterDocument = BsonSerializer.Deserialize<BsonDocument>(filterString);
            var filter = new BsonDocumentFilterDefinition<Post>(filterDocument);
            return _postCollection.Find(filter).ToListAsync();

        }

        public Task<Post> GetbyId(string id)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Id, id);
            return _postCollection.Find(filter).FirstOrDefaultAsync();
        }

        public Task UpdatebyInstance(Post post)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Id, post.Id);
            return _postCollection.FindOneAndReplaceAsync(filter, post);
        }

        public Task UpdatebyParameters(string id, List<UpdateParameter> parameters)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Id, id);
            var updateBuilder = Builders<Post>.Update;
            List<UpdateDefinition<Post>> subUpdates = new List<UpdateDefinition<Post>>();
            foreach (var parameter in parameters)
            {
                switch (parameter.updateAction)
                {
                    case UpdateAction.set:
                        subUpdates.Add(Builders<Post>.Update.Set(parameter.FieldName, parameter.Value));
                        break;
                    case UpdateAction.push:
                        subUpdates.Add(Builders<Post>.Update.Push(parameter.FieldName, parameter.Value));
                        break;
                    case UpdateAction.pull:
                        subUpdates.Add(Builders<Post>.Update.Pull(parameter.FieldName, parameter.Value));
                        break;
                }
            }
            var combinedUpdate = updateBuilder.Combine(subUpdates);
            return _postCollection.UpdateOneAsync(filter, combinedUpdate);
        }
    }
}