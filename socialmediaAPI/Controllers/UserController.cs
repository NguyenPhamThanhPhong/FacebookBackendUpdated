using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using socialmediaAPI.Configs;
using socialmediaAPI.Models.DTO;
using socialmediaAPI.Models.Embeded.User;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.Repositories.Repos;
using socialmediaAPI.RequestsResponses.Requests;
using socialmediaAPI.Services.CloudinaryService;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace socialmediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly CloudinaryHandler _cloudinaryHandler;
        private readonly string _userFolderName;
        private readonly IMongoCollection<User> _userCollection;


        public UserController(IMapper mapper, IUserRepository userRepository, CloudinaryHandler cloudinaryHandler,
            CloudinaryConfigs cloudinaryConfigs, DatabaseConfigs databaseConfigs, IConversationRepository conversationRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _cloudinaryHandler = cloudinaryHandler;
            _userFolderName = cloudinaryConfigs.UserFolderName;
            _userCollection = databaseConfigs.UserCollection;
            _conversationRepository = conversationRepository;
        }
        [HttpGet("/viewDTO/{id}")]
        public async Task<IActionResult> GetUserDTO(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest("invalid id");
            var user = await _userRepository.GetbyId(id);
            var userDTO = _mapper.Map<UserDTO>(user);
            return Ok(userDTO);
        }

        [HttpPost("/get-from-ids")]
        public async Task<IActionResult> GetFromIds([FromBody] List<string> ids)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var filter = Builders<User>.Filter.In(s => s.ID, ids);
            var users = await _userCollection.Find(filter).ToListAsync();
            return Ok(users);
        }


        [HttpPost("/friend-suggest")]
        public async Task<IActionResult> SuggestFromIds([FromBody] List<string> ids)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var filter = Builders<User>.Filter.Nin(u => u.ID, ids);
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", filter.ToBsonDocument()),
                new BsonDocument("$sample", new BsonDocument("size", 40))
            };
            var randomUser = await _userCollection.Aggregate<User>(pipeline).FirstOrDefaultAsync();
            if(randomUser!=null)
            {
                return Ok(randomUser);
            }
            return Ok(new List<User>());
        }

        [HttpPost("/friend-search")]
        public async Task<IActionResult> GetPeople([FromBody] string search)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var pattern = new BsonRegularExpression(new Regex(Regex.Escape(search), RegexOptions.IgnoreCase));

            var filter = Builders<User>.Filter.Regex(s=>s.PersonalInfo.Name,pattern);
            var people = await _userCollection.Find(filter).ToListAsync();
            var peopleDTO = _mapper.Map<List<UserDTO>>(people);
            return Ok(peopleDTO);
        }

        [HttpGet]
        #region email-password

        [HttpPost("/update-email/{id}")]
        public async Task<IActionResult> UpdateEmail(string id, [FromBody] string email)
        {
            if (!ModelState.IsValid)
                return BadRequest($"invalid model state");
            var parameter = new UpdateParameter(Models.Entities.User.GetFieldName(u => u.AuthenticationInfo.Email), email, UpdateAction.set);
            await _userRepository.UpdatebyParameters(id, new List<UpdateParameter> { parameter });
            return Ok("updated");
        }
        [HttpPost("/update-password/{username}")]
        public async Task<IActionResult> UpdatePassword(string username, [FromBody] string password)
        {
            if (!ModelState.IsValid)
                return BadRequest($"invalid model state");
            var filter = Builders<User>.Filter.Eq(s=>s.AuthenticationInfo.Username,username);
            var update = Builders<User>.Update.Set(s=>s.AuthenticationInfo.Password,password);
            await _userCollection.UpdateOneAsync(filter, update);
            return Ok();
        }
        #endregion

        [HttpPost("/update-personal-info/{id}")]
        public async Task<IActionResult> UpdatePersonalInfo(string id, [FromForm] UpdateUserPersonalInformationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("invalid modelstate");
            var personalInfo = request.ConvertToPersonalInformation();
            if (!string.IsNullOrEmpty(request.prevAvatar))
                await _cloudinaryHandler.Delete(request.prevAvatar);
            if (request.AvatarFile != null)
                personalInfo.AvatarUrl = await _cloudinaryHandler.UploadSingleImage(request.AvatarFile, _userFolderName);

            var filter = Builders<User>.Filter.Eq(s => s.ID, id);
            var update = Builders<User>.Update.Set(s => s.PersonalInfo, personalInfo);
            var result = await _userCollection.UpdateOneAsync(filter, update);
            if (result.ModifiedCount > 0)
                return Ok("updated");
            return BadRequest("failed to update");

        }


        [HttpDelete("/user-delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest($"invalid model state ");
            var deletedUser = await _userRepository.Delete(id);
            await _cloudinaryHandler.Delete(deletedUser.PersonalInfo.AvatarUrl);
            return Ok("deleted");
        }


        [Authorize]
        [HttpPost("/user-update-friend-request/{targetId}/{option}")]
        public async Task<IActionResult> UpdateFriendRequest(string targetId, UpdateAction option)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var selfId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (selfId == null)
                return Unauthorized("no id found");
            var filter = Builders<User>.Filter.Eq(s => s.ID, targetId);
            var selfFilter = Builders<User>.Filter.Eq(s => s.ID, selfId);
            if (option == UpdateAction.push)
            {
                var updateTarget = Builders<User>.Update.Push(s => s.FriendRequestIds, selfId);
                var updateSelf = Builders<User>.Update.Push(s => s.FriendWaitIds, targetId);
                await Task.WhenAll(_userCollection.UpdateOneAsync(filter, updateTarget),
                    _userCollection.UpdateOneAsync(selfFilter, updateSelf));
            }
            else
            {
                var update = Builders<User>.Update.Pull(s => s.FriendRequestIds, selfId);
                var updateSelf = Builders<User>.Update.Pull(s => s.FriendWaitIds, targetId);
                await Task.WhenAll(_userCollection.UpdateOneAsync(filter, update),
                    _userCollection.UpdateOneAsync(selfFilter, updateSelf));
            }
            return Ok();
        }

        [Authorize]
        [HttpPost("/user-unfriend-accept-request/{targetId}/{option}")]
        public async Task<IActionResult> UpdateFriendList(string targetId, ConversationCreateRequestFriend request, UpdateAction option)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var selfId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (selfId == null)
                return Unauthorized("no id found");
            var targetfilter = Builders<User>.Filter.Eq(s => s.ID, targetId);
            var selfFilter = Builders<User>.Filter.Eq(s => s.ID, selfId);
            if (option == UpdateAction.push) // accept friend request
            {
                var targetUpdate = Builders<User>.Update.Pull(s => s.FriendWaitIds, targetId).Push(s => s.FriendIds, targetId);
                var updateSelf = Builders<User>.Update.Pull(s => s.FriendRequestIds, targetId).Push(s => s.FriendIds, targetId);
                Conversation conversation = new Conversation()
                {
                    Name = request.Name,
                    AdminIDs = new List<string> { targetId, selfId },
                    AvatarUrl = request.AvatarUrl,
                    IsGroup = false,
                    ParticipantIds = new List<string> { selfId, targetId },
                    MessageIds = new List<string>(),
                    RecentMessage = string.Empty,
                    RecentTime = DateTime.Now
                };
                await Task.WhenAll(_userCollection.UpdateOneAsync(targetfilter, targetUpdate),
                    _userCollection.UpdateOneAsync(selfFilter, updateSelf));
            }
            else // un-friend
            {
                var targetUpdate = Builders<User>.Update.Pull(s => s.FriendWaitIds, selfId);
                var updateSelf = Builders<User>.Update.Pull(s => s.FriendRequestIds, targetId);
                await Task.WhenAll(_userCollection.UpdateOneAsync(targetfilter, targetUpdate),
                    _userCollection.UpdateOneAsync(selfFilter, updateSelf));
            }
            return Ok();
        }

        [Authorize]
        [HttpPost("/user-update-block-list/{targetId}/{option}")]
        public async Task<IActionResult> UpdateBlock(string targetId, UpdateAction option)
        {
            var selfId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (selfId == null)
                return Unauthorized("no id found");
            var selfFilter = Builders<User>.Filter.Eq(s => s.ID, selfId);
            if (option == UpdateAction.push)
            {
                var selfUpdate = Builders<User>.Update.Push(s => s.BlockedIds, targetId);
                await _userCollection.UpdateOneAsync(selfFilter, selfUpdate);
            }
            else
            {
                var selfUpdate = Builders<User>.Update.Pull(s => s.BlockedIds, targetId);
                await _userCollection.UpdateOneAsync(selfFilter, selfUpdate);
            }
            return Ok();
        }


        #region private Util function update user Database & Cloudinary
        private async Task UpdateUserAvatar(string id, List<IFormFile> files)
        {
            var avatarSet = await _cloudinaryHandler.UploadImages(files, _userFolderName);
            UpdateParameter parameter = new UpdateParameter()
            {
                FieldName = Models.Entities.User.GetFieldName(u => u.PersonalInfo.AvatarUrl),
                Value = avatarSet.Values.FirstOrDefault(),
                updateAction = UpdateAction.set
            };
            await _userRepository.UpdatebyParameters(id, new List<UpdateParameter> { parameter });
        }
        #endregion
    }
}
//[HttpPut("/update-parameters-string-fields/{id}")]
//public async Task<IActionResult> UpdateParmeters(string id, [FromBody] List<UpdateParameter> parameters)
//{
//    if (!ModelState.IsValid)
//        return BadRequest("invalid modelstate");
//    await _userRepository.UpdateStringFields(id, parameters);
//    return Ok("updated");
//}


//[HttpPut("/update-avatar/{id}/{prevUrl}")]
//public async Task<IActionResult> UpdateAvatar(string id, string prevUrl , [FromForm] IFormFile file)
//{
//    if (!ModelState.IsValid)
//        return BadRequest("invalid modelstate");
//    var avatarParameter = new UpdateParameter()
//    {
//        FieldName = Models.Entities.User.GetFieldName(u => u.PersonalInfo.AvatarUrl),
//        updateAction = UpdateAction.set
//    };
//    if (file == null)
//    {
//        if(!string.IsNullOrEmpty(prevUrl))
//            await _cloudinaryHandler.Delete(prevUrl);
//        avatarParameter.Value = null;
//    }
//    else
//        avatarParameter.Value = await _cloudinaryHandler.UploadSingleImage(file, _userFolderName);

//    await _userRepository.UpdatebyParameters(id,new List<UpdateParameter> { avatarParameter });
//    return Ok("updated");
//}
