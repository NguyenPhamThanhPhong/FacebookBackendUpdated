using AutoMapper;
using CloudinaryDotNet;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.Repositories.Repos;
using socialmediaAPI.RequestsResponses.Requests;
using socialmediaAPI.Services.CloudinaryService;
using System.Text.RegularExpressions;

namespace socialmediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly CloudinaryHandler _cloudinaryHandler;
        private readonly IMapper _mapper;
        private readonly string _conversationFolderName = "Conversation";

        public ConversationController(IConversationRepository conversationRepository, CloudinaryHandler cloudinaryHandler, IMapper mapper, IMessageRepository messageRepository)
        {
            _conversationRepository = conversationRepository;
            _cloudinaryHandler = cloudinaryHandler;
            _mapper = mapper;
            _messageRepository = messageRepository;
        }
        [HttpPost("/conversation-create")]
        public async Task<IActionResult> Create([FromForm] ConversationCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var conversation = _mapper.Map<Conversation>(request);
            if (request.File != null)
                conversation.AvatarUrl = await _cloudinaryHandler.UploadSingleImage(request.File, _conversationFolderName);
            await _conversationRepository.Create(conversation);
            return Ok(conversation);
        }

        [HttpPost("/conversation-get-from-ids/{skip}")]
        public async Task<IActionResult> GetMany([FromBody] List<string> ids, int skip)
        {
            if (!ModelState.IsValid)
                return BadRequest("invalid modelstate");
            var conversations = await _conversationRepository.GetbyIds(ids, skip);
            return Ok(conversations);
        }

        [HttpPost("/conversation-search")]
        public async Task<IActionResult> GetbyName(string search)
        {
            if (!ModelState.IsValid)
                return BadRequest("invalid modelstate");
            var pattern = new BsonRegularExpression(new Regex(Regex.Escape(search), RegexOptions.IgnoreCase));

            var filter = Builders<Conversation>.Filter.Regex(c => c.Name, pattern);
            var conversations = await _conversationRepository.GetbyFilter(filter);
            return Ok(conversations);

        }

        [HttpPost("/conversation-update-avatar/{id}")]
        public async Task<IActionResult> UpdateAvatar(string id,IFormFile file)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            if (file == null)
                return BadRequest();

            string? avatarUrl = await _cloudinaryHandler.UploadSingleImage(file, _conversationFolderName);
            var filter = Builders<Conversation>.Filter.Eq(s => s.ID, id);
            var update = Builders<Conversation>.Update.Set(s => s.AvatarUrl, avatarUrl);
            return Ok();
        }



    }
}
