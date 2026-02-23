using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using testforproject.Authen;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Controllers.API.Profile
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileApiController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        public ProfileApiController(ApplicationDbContext db, TokenProvider tokenProvider, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _db = db;
        }
        [HttpPost("Edit")]
        public IActionResult Edit([FromBody] User data)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");

            }
            var user = _db.Users.FirstOrDefault(u => u.Uid == int.Parse(userId));
            user.DisplayName = data.DisplayName;
            user.Bio = data.Bio;
            user.Gender = data.Gender;
            user.Age = data.Age;
            _db.SaveChanges();
            
            return Ok(new {message = "email" });
        }
        [HttpPost("Follow")]
        public IActionResult Follow([FromBody] FollowRequest req)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var currentUser = _db.Users
                .Include(u => u.Following)
                .FirstOrDefault(u => u.Uid == int.Parse(userId));

            var targetUser = _db.Users.Find(req.TargetUserId);

            if (currentUser == null || targetUser == null)
                return NotFound();

            if (!currentUser.Following.Contains(targetUser))
            {
                currentUser.Following.Add(targetUser);
                _db.SaveChanges();
            }

            var followerCount = _db.Users
                .Include(u => u.Follower)
                .First(u => u.Uid == targetUser.Uid)
                .Follower.Count;

            return Ok(new
            {
                success = true,
                isFollowing = true,
                followerCount = followerCount
            });
        }

        [HttpPost("Unfollow")]
        public IActionResult Unfollow([FromBody] FollowRequest req)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var currentUser = _db.Users
                .Include(u => u.Following)
                .FirstOrDefault(u => u.Uid == int.Parse(userId));

            var targetUser = _db.Users.Find(req.TargetUserId);

            if (currentUser == null || targetUser == null)
                return NotFound();

            if (currentUser.Following.Contains(targetUser))
            {
                currentUser.Following.Remove(targetUser);
                _db.SaveChanges();
            }

            var followerCount = _db.Users
                .Include(u => u.Follower)
                .First(u => u.Uid == targetUser.Uid)
                .Follower.Count;

            return Ok(new
            {
                success = true,
                isFollowing = false,
                followerCount = followerCount
            });
        }














        public class FollowRequest
        {
            public int TargetUserId { get; set; }
        }

    }
}
