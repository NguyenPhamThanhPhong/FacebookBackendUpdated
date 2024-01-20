using AutoMapper;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using socialmediaAPI.Configs;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.RequestsResponses.Requests;
using socialmediaAPI.Services.CloudinaryService;
using System.Text.RegularExpressions;

namespace socialmediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly CloudinaryHandler _cloudinaryHandler;
        private readonly IMapper _mapper;
        private readonly string _messageFolderName;
        private readonly IMongoCollection<Message> _messageCollection;

        public MessageController(IMessageRepository messageRepository, 
            CloudinaryHandler cloudinaryHandler, IMapper mapper, DatabaseConfigs databaseConfigs,
            CloudinaryConfigs cloudinaryConfigs)
        {
            _messageRepository = messageRepository;
            _cloudinaryHandler = cloudinaryHandler;
            _mapper = mapper;
            _messageFolderName = cloudinaryConfigs.MessageFolderName;
            _messageCollection = databaseConfigs.MessageCollection;
        }
        [HttpPost("/message-send")]
        public async Task<IActionResult> Create([FromForm] MessageCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var message = _mapper.Map<Message>(request);
            if (request.Files != null)
                message.FileUrls = await _cloudinaryHandler.UploadImages(request.Files, _messageFolderName);
             await _messageRepository.Create(message);
            return Ok(message);
        }

        [HttpDelete("/message-delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var deletedMessage  = await _messageRepository.Delete(id);
            if (deletedMessage.FileUrls != null)
                await _cloudinaryHandler.DeleteMany(deletedMessage.FileUrls.Values.ToList());

            return Ok($"delete state is {deletedMessage!=null}");
        }

        [HttpPost("/message-get-many/{skip}")]
        public async Task<IActionResult> GetMany([FromBody] List<string> messageIds, int skip)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var messages = await _messageRepository.GetbyIds(messageIds, skip);
            return Ok(messages);
        }
        [HttpPost("/message-search")]
        public async Task<IActionResult> GetSearch([FromBody] string search)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var pattern = new BsonRegularExpression(new Regex(Regex.Escape(search), RegexOptions.IgnoreCase));

            var filter = Builders<Message>.Filter.Regex(msg => msg.Content, pattern);
            var messages = await _messageCollection.Find(filter).ToListAsync();
            return Ok(messages);
        }

    }
}
//[HttpPost("/message-update")]
//public async Task<IActionResult> UpdateMessage(string id, [FromBody] string content)
//{
//    if (!ModelState.IsValid)
//        return BadRequest();
//    var filter = Builders<Message>.Filter.Eq(s=>s.Id, id);
//    var update = Builders<Message>.Update.Set(s=>s.Content, content);
//    await _messageRepository.UpdateContent()
//    return Ok("updated");
//}