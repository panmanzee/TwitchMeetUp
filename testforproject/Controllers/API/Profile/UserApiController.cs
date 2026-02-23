using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using testforproject.Authen;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Controllers.API.Profile
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly TokenProvider _tokenProvider;

        public UserApiController(ApplicationDbContext db, TokenProvider tokenProvider)
        {
            _db = db;
            _tokenProvider = tokenProvider;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            Console.WriteLine(user);
            if (!ModelState.IsValid)
                return BadRequest(new { success = false });
            user.HostScore = 5;
            user.ParticipateScore = 5;
            _db.Users.Add(user);
            _db.SaveChanges();
            string token = _tokenProvider.Create(user);Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,      
                Secure = false,        
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(29)
            });
            
            return Ok(new { success = true });
        }
        [HttpPost("upload")]
        public async Task<IActionResult> upload(IFormFile file)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");

            }
            var user = _db.Users.FirstOrDefault(u => u.Uid == int.Parse(userId));
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var filePath = Path.Combine("wwwroot/imageProfile", user.Uid.ToString()+".jpg");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            user.ProfilePictureSrc = "/imageProfile/" + user.Uid + ".jpg";
            _db.SaveChanges();
            return Ok(new { message = "Upload success", fileName = user.Uid });


        }
    }
}
