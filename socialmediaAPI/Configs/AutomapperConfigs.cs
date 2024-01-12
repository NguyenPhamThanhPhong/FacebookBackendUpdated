using AutoMapper;
using socialmediaAPI.Models.DTO;
using socialmediaAPI.Models.Embeded.Post;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.RequestsResponses.Requests;

namespace socialmediaAPI.Configs
{
    public class AutomapperConfigs : Profile
    {
        public AutomapperConfigs()
        {
            CreateMap<User, UserDTO>();
            CreateMap<ConversationCreateRequest, Conversation>();
            CreateMap<MessageCreateRequest, Message>();
        }
    }
}
