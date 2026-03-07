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
        private readonly testforproject.Features.Notification.INotification _notiService;
        public ProfileApiController(ApplicationDbContext db, TokenProvider tokenProvider, IJwtService jwtService, testforproject.Features.Notification.INotification notiService)
        {
            _jwtService = jwtService;
            _db = db;
            _notiService = notiService;
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
            if (user == null) return NotFound();

            user.DisplayName = data.DisplayName;
            user.Bio = data.Bio;

            // Only allow setting Age and Gender if they are currently null
            if (user.Age == null)
            {
                user.Age = data.Age;
            }
            if (string.IsNullOrEmpty(user.Gender))
            {
                user.Gender = data.Gender;
            }

            _db.SaveChanges();

            return Ok(new { message = "email" });
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

                // Notify target user
                var title = "New Follower!";
                var desc = $"{currentUser.DisplayName ?? currentUser.Username} is now following you.";
                var date = DateTime.Now.ToString("dd MMM yyyy HH:mm");
                // Href to the follower's profile
                var href = $"http://localhost:5189/Profile/Index/{currentUser.Uid}#";

                _notiService.CreateNotification(title, desc, date, new List<User> { targetUser }, href, currentUser.Uid);
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
