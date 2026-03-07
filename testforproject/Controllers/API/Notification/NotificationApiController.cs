using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Features.Notification;
using testforproject.Models;

namespace testforproject.Controllers.API.Notification
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotiApiController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly INotification _Noti;
        public NotiApiController(ApplicationDbContext db, IJwtService jwtService, INotification notification)
        {
            _db = db;
            _jwtService = jwtService;
            _Noti = notification;
        }
        [HttpPost("CreateNoti")]
        public async Task<IActionResult> CreateNoti(string Title, string Description, string Date, int Uid)
        {

            var followers = _db.Users
             .Where(u => u.Uid == Uid)
             .SelectMany(u => u.Follower)
             .ToList();
            await _Noti.CreateNotification(Title, Description, Date, followers);
            return Ok(new { count = followers.Count });

        }
        [HttpGet("GetNoti")]
        public async Task<IActionResult> GetNoti()
        {
            var userId = _jwtService.UserId;
            if (userId == null)
                return Unauthorized();

            var notis = await _db.Notifications
                .Where(n => n.UserUid == userId)
                .OrderByDescending(n => n.Date)
                .Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    description = n.Description,
                    isReaded = n.IsReaded,
                    date = n.Date,
                    href = n.Href,

                    user = new
                    {
                        username = n.User.Username
                    },
                    // Manual lookup to avoid navigation property ambiguity
                    triggerUser = _db.Users
                        .Where(u => u.Uid == n.TriggerUserUid)
                        .Select(u => new
                        {
                            username = u.Username,
                            profileImage = u.ProfilePictureSrc
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(notis);
        }
        [HttpPut("MarkRead/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = _jwtService.UserId;
            if (userId == null)
                return Unauthorized();

            var noti = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserUid == userId);

            if (noti == null)
                return NotFound("Notification not found");

            noti.IsReaded = true;
            await _db.SaveChangesAsync();

            return Ok(noti);
        }
    }

}
