using socialmediaAPI.Models.Entities;
using socialmediaAPI.RequestsResponses.Requests;

namespace socialmediaAPI.Repositories.Interface
{
    public interface IPostRepository
    {
        public Task CreatePost(Post post);

        public Task<IEnumerable<Post>> GetbyIds(IEnumerable<string> ids);
        public Task<List<Post>> GetbyFilterString(string filterString);

        public Task UpdatebyInstance(Post post);
        public Task UpdatebyParameters(string id, List<UpdateParameter> parameters);
        public Task UpdateStringFields(string id, List<UpdateParameter> parameters);

        public Task<Post> Delete(string id);
    }
}
