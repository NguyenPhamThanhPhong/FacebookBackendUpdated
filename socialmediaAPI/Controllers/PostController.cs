using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using socialmediaAPI.Configs;
using socialmediaAPI.Models.Embeded.Post;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.RequestsResponses.Requests;
using socialmediaAPI.Services.CloudinaryService;

namespace socialmediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly CloudinaryHandler _cloudinaryHandler;
        private readonly string _postFolderName;
        private readonly IMongoCollection<Post> _postCollection;

        public PostController(IPostRepository postRepository, DatabaseConfigs databaseConfigs,
            CloudinaryHandler cloudinaryHandler,CloudinaryConfigs cloudinaryConfigs)
        {
            _postRepository = postRepository;
            _cloudinaryHandler = cloudinaryHandler;
            _postFolderName = cloudinaryConfigs.PostFolderName;
            _postCollection = databaseConfigs.PostCollection;
        }
        [HttpPost("/post-create")]
        public async Task<IActionResult> Create([FromForm] CreatePostRequest request )
        {
            if (!ModelState.IsValid)
                return BadRequest("invalid modelstate");
            Post post = request.ConvertToPost();

            if (request.Files != null)
            {
                var fileUrls = await _cloudinaryHandler.UploadImages(request.Files, _postFolderName);
                post.FileUrls = fileUrls;
            }
            await _postRepository.CreatePost(post);
            return Ok(post);
        }

        [HttpPost("/post-like-unlike/{id}/{updateAction}")]
        public async Task<IActionResult> UpdateLikes(string id, UpdateAction updateAction,[FromBody] LikeRepresentation likeRepresentation )
        {
            if (!ModelState.IsValid || updateAction == UpdateAction.set)
                return BadRequest("invalid modelstate");
            var parameter = new UpdateParameter(Post.GetFieldName(p=>p.Likes),likeRepresentation,updateAction);
            await _postRepository.UpdatebyParameters(id, new List<UpdateParameter> { parameter });
            return Ok("updated");
        }

        [HttpPost("/post-get-from-ids")]
        public async Task<IActionResult> Get([FromBody] List<string> ids)
        {
            if (!ModelState.IsValid)
                return BadRequest("invalid modelstate");
            var posts = await _postRepository.GetbyIds(ids);
            return Ok(posts);
        }
        [HttpPost("/post-update/{id}")]
        public async Task<IActionResult> UpdateImages(string id, [FromForm] UpdatePostRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("invalid modelstate");
            if (request.deleteUrls != null)
                await _cloudinaryHandler.DeleteMany(request.deleteUrls);

            var fileUrls = request.keepUrls;
            if (request.Files != null)
             fileUrls = await _cloudinaryHandler.UploadImages(request.Files,_postFolderName);

            var filter = Builders<Post>.Filter.Eq(s=>s.Id,id);
            var update = Builders<Post>.Update.Set(s=>s.Content,request.Content).Set(s=>s.FileUrls,fileUrls);
            await _postCollection.UpdateOneAsync(filter, update);
            return Ok("updated");
        }
        [HttpDelete("/post-delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest("invalid modelstate");
            var deleted = await _postRepository.Delete(id);
            if (deleted.FileUrls != null)
                await _cloudinaryHandler.DeleteMany(deleted.FileUrls.Values.ToList());
            return Ok(("deleted", deleted));
        }

    }
}
//[HttpPost("/post-update-string-field/{id}")]
//public async Task<IActionResult> UpdateParameters(string id, [FromBody] List<UpdateParameter> parameters)
//{
//    if (!ModelState.IsValid)
//        return BadRequest("invalid modelstate");
//    await _postRepository.UpdateStringFields(id, parameters);
//    return Ok("updated");
//}