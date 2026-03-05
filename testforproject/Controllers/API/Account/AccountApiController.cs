using Azure;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using testforproject.Authen;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;
using Microsoft.EntityFrameworkCore;
namespace testforproject.Controllers.API.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly TokenProvider _tokenProvider;
        private readonly IJwtService _jwtService;
        public AccountApiController(ApplicationDbContext db, TokenProvider tokenProvider, IJwtService jwtService)
        {
            _db = db;
            _tokenProvider = tokenProvider;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrEmpty(user.Password) || string.IsNullOrWhiteSpace(user.ConfirmPassword))
            {
                return BadRequest(new { message = "All fields required" });
            }
            if (user.Password != user.ConfirmPassword)
            {
                return BadRequest(new { message = "Password do not match" });
            }
            if (_db.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }
            _db.Users.Add(user);
            _db.SaveChanges();

            string token = _tokenProvider.Create(user);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            return Ok(new { message = " Registered successfully", loggedIn = true });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User login_user)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == login_user.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "Username not Found" });
            }
            if (user.Password != login_user.Password)
            {
                return Unauthorized(new { message = "Password Incorrect" });
            }
            string token = _tokenProvider.Create(user);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            return Ok(new { message = "Login Successfully" });
        }


        // For checking login (is it already login)
        [HttpGet("me")]
        public IActionResult me()
        {
            var token = _jwtService.UserId;
            if (token == null)
            {
                return Unauthorized();
            }
            return Ok(new { loggedIn = true });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged out" });
        }

        [HttpPost("onboarding")]
        public IActionResult Onboarding([FromBody] List<int> categoryIds)
        {
            var userId = _jwtService.UserId;
            if (userId == null)
            {
                return Unauthorized(new { message = "You must be logged in to complete onboarding." });
            }

            if (categoryIds == null || categoryIds.Count != 5)
            {
                return BadRequest(new { message = "Please select exactly 5 categories." });
            }

            var user = _db.Users.Include(u => u.PreferredCategories).FirstOrDefault(u => u.Uid == userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Fetch selected categories from DB
            var selectedCategories = _db.Categories.Where(c => categoryIds.Contains(c.Id)).ToList();

            user.PreferredCategories?.Clear();
            foreach (var cat in selectedCategories)
            {
                user.PreferredCategories?.Add(cat);
            }

            _db.SaveChanges();

            return Ok(new { message = "Onboarding completed successfully" });
        }


        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleTokenDto dto)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { "381740202292-rtg3q475g4njabu97cei3pdk4cnurcd3.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.Token, settings);

                var user = _db.Users.FirstOrDefault(u => u.Email == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Username = payload.Name,
                        Email = payload.Email,
                        DisplayName = payload.Name,
                        Password = Guid.NewGuid().ToString(),
                        ProfilePictureSrc = payload.Picture,
                        HostScore = 0,
                        ParticipateScore = 0
                    };
                    _db.Users.Add(user);
                    await _db.SaveChangesAsync();
                }

                user.Email ??= payload.Email;
                user.Username ??= payload.Name;

                string token = _tokenProvider.Create(user);

                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return Ok(new { message = "Login with Google Successfully" });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = "Google token invalid", error = ex.Message });
            }
        }

    }

}


