using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using testforproject.Authen;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Controllers.API.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly TokenProvider _tokenProvider;
        private readonly IJwtService  _jwtService;
        public AccountApiController(ApplicationDbContext db, TokenProvider tokenProvider,IJwtService jwtService)
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
                HttpOnly = true ,
                SameSite = SameSiteMode.None ,
                Secure = true ,
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

            Response.Cookies.Append("jwt", token, new CookieOptions {
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
            var token = Request.Cookies["jwt"];

           
            return Ok(new {message = "Logged out"});
        }

    }

}


