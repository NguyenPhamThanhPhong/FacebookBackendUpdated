using socialmediaAPI.Models.Embeded.User;
using socialmediaAPI.Models.Entities;

namespace socialmediaAPI.RequestsResponses.Requests
{
    public class UpdateUserPersonalInformationRequest
    {
        public string? Name { get; set; }
        public string? prevAvatar { get; set; }
        public string? prevPhoto { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public IFormFile? CoverPhotoFile { get; set; }
        public DateTime? DateofBirth { get; set; }
        public string LiveAt { get; set; }
        public string StudyAt { get; set; }
        public string Address { get; set; }
        public string RelationShip { get; set; }
        public string Phone { get; set; }
        public string? Favorites { get; set; }
        public string? Biography { get; set; }
        public UpdateUserPersonalInformationRequest()
        {
            Name = "";
            LiveAt = "";
            StudyAt = "";
            Address = "";
            RelationShip = "";
            Phone = "";
        }

        public PersonalInformation ConvertToPersonalInformation()
        {
            return new PersonalInformation()
            {
                Name = Name,
                AvatarUrl = "",
                DateofBirth = DateofBirth,
                Favorites= Favorites,
                Biography= Biography,
                LiveAt = LiveAt, StudyAt = StudyAt,
                Address = Address, RelationShip = RelationShip,
                Phone = Phone
            };
        }
    }
}
