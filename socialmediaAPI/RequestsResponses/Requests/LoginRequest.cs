using System.ComponentModel.DataAnnotations;

namespace socialmediaAPI.RequestsResponses.Requests
{
    public class LoginRequest
    {
        [Required]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.")]
        public string Username { get; set; }
        [Required]
        [StringLength(32, MinimumLength = 4, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.")]
        public string Password { get; set; }
        public LoginRequest()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

    }
}
