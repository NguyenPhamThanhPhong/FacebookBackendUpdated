using socialmediaAPI.Models.Embeded.User;
using socialmediaAPI.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace socialmediaAPI.RequestsResponses.Requests
{
#pragma warning disable CS8618

    public class UserRegisterRequest
    {
        [Required]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.")]
        public string Username { get; set; }
        [StringLength(32, MinimumLength = 4, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.")]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }

        public User ConvertToUser()
        {
            return new User()
            {
                IsMailConfirmed = false,
                AuthenticationInfo = new AuthenticationInformation()
                {
                    Username = Username,
                    Password = Password,
                    Email = Email,
                },
                PersonalInfo = new PersonalInformation()
                {
                    Name = Name,
                    DateofBirth = this.DateOfBirth,
                    Favorites = "",
                    Biography = "",
                    AvatarUrl = ""
                }
            };
        }
    }
}
