using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using socialmediaAPI.Configs;
using socialmediaAPI.Models.DTO;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.RequestsResponses.Requests;
using socialmediaAPI.Services.Authentication;
using socialmediaAPI.Services.CloudinaryService;
using socialmediaAPI.Services.SMTP;
using socialmediaAPI.Services.Validators;
using System.Security.Claims;

namespace socialmediaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserValidator _userValidator;
        private readonly CloudinaryHandler _cloudinaryHandler;
        private readonly string _userFolderName;
        private readonly EmailUtil _emailUtil;
        private readonly TokenGenerator _tokenGenerator;


        public LoginController(IUserRepository userRepository, UserValidator userValidator,
            CloudinaryHandler cloudinaryHandler, CloudinaryConfigs cloudinaryConfigs,
            EmailUtil emailUtil, TokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _userValidator = userValidator;
            _cloudinaryHandler = cloudinaryHandler;
            _userFolderName = cloudinaryConfigs.UserFolderName;
            _emailUtil = emailUtil;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("invalid modelstate");
            }
            if(! await _emailUtil.SendEmailAsync(request.Email,$"Welcome {request.Name} to PhongBook","Welcome..."))
            {
                return BadRequest("email doesn't exists");
            }

            var user = request.ConvertToUser();
            try
            {
                await _userRepository.Create(user);
            }
            catch (Exception ex)
            {
                return BadRequest("user already existed: \n"+ ex.Message);
            }
            return Ok(user);
        }
        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Incorrect username or password");
            var user = await _userRepository.GetbyUsername(request.Username);
            if (user == null || !(user.AuthenticationInfo.Password == request.Password))
                return BadRequest("Incorrect username or password");

            string accessToken = _tokenGenerator.GenerateAccessToken(user);
            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(120) // Cookie expiration time
            };
            Response.Cookies.Append("access_token", accessToken, cookieOptions );
            Response.Cookies.Append("userID", user.ID, cookieOptions);
            return Ok(accessToken);
        }
        [Authorize]
        [HttpGet("/login-auto")]
        public async Task<IActionResult> LoginAuto()
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == null)
                return Unauthorized();

            var user = await _userRepository.GetbyId(id);

            return Ok(user);
        }

        [HttpPost("/send-mail-verification")]
        public async Task<IActionResult> SendVerification([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userRepository.GetbyUsername(request.Username);
            if (user == null || !(user.AuthenticationInfo.Password == request.Password))
                return BadRequest("Incorrect username or password");

            Random random = new Random();
            string codeValue = random.Next(100000, 999999).ToString();
            var result = await _emailUtil.SendEmailAsync(user.AuthenticationInfo.Email, 
                "No reply: your email verification code is", codeValue);
            user.EmailVerification = new Models.Embeded.User.VerificationTicket
            {
                Code = codeValue,
                ExpiredTime = DateTime.UtcNow.AddMinutes(15)
            };
            await _userRepository.UpdatebyInstance(user);
            return Ok($"sending result is {result}");
        }
        [HttpPost("/confirm-mail/{id}")]
        public async Task<IActionResult> ConfirmEmail(string id,[FromBody]string code)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userRepository.GetbyId(id);
            if (user == null)
                return BadRequest("invalid user");
            if (user.EmailVerification != null && user.EmailVerification.Code == code)
            {
                user.IsMailConfirmed = true;
                await _userRepository.UpdatebyInstance(user);
                return Ok("confirmed");
            }
            return BadRequest("invalid code");
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
