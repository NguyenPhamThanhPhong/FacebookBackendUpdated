using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using socialmediaAPI.Configs;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.RequestsResponses.Requests;
using socialmediaAPI.Services.CloudinaryService;
using System.Text.Json;

namespace socialmediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly CloudinaryHandler _cloudinaryHandler;
        private readonly string _commentFolderName;
        private readonly IMongoCollection<Comment> _commentCollection;
        private readonly IMapper _mapper;

        public CommentController(CloudinaryHandler cloudinaryHandler, CloudinaryConfigs cloudinaryConfigs,
            ICommentRepository commentRepository, DatabaseConfigs databaseConfigs, IMapper mapper)
        {
            _cloudinaryHandler = cloudinaryHandler;
            _commentFolderName = cloudinaryConfigs.CommentFolderName;
            _commentRepository = commentRepository;
            _commentCollection = databaseConfigs.CommentCollection;
            _mapper = mapper;
        }

        [HttpPost("/comment-get-many/{skip}")]
        public async Task<IActionResult> GetMany([FromBody] List<string> ids,int skip)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var comments = await _commentRepository.GetfromIds(ids, skip);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("/comment-create")]
        public async Task<IActionResult> CreateComment([FromForm] CreateCommentRequest request)
        {
            Console.WriteLine(JsonSerializer.Serialize(request));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var comment = _mapper.Map<Comment>(request);
            await _commentRepository.Create(comment);
            return Ok(comment);
        }

        [HttpPost("/comment-update")]
        public async Task<IActionResult> Update([FromForm] UpdateCommentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var filter = Builders<Comment>.Filter.Eq(s=>s.Id, request.Id);
            var update = Builders<Comment>.Update.Set(s=>s.Content,request.Content);
            if (request.Files != null && request.Files.Count > 0)
            {
                var urls = await _cloudinaryHandler.UploadImages(request.Files, _commentFolderName);
                update.Set(s => s.FileUrls, urls);
            }
            var result = await _commentCollection.UpdateOneAsync(filter, update);
            return Ok(result.ModifiedCount>0);
        }

        [HttpDelete("/comment-delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var deletedComment = await _commentCollection.FindOneAndDeleteAsync(s => s.Id == id);
            if(deletedComment!=null && deletedComment.FileUrls != null )
            {
                List<string?> urls = deletedComment.FileUrls.Values.ToList();
                await _cloudinaryHandler.DeleteMany(urls);
            }
            return Ok("deleted");
        }



    }
}
