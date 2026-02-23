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
            int userIdStr = 3;
            if (userIdStr == null)
                return Unauthorized();

            int userId =(userIdStr);

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

                    user = new
                    {
                        username = n.User.Username
                    }
                })
                .ToListAsync();

            return Ok(notis);
        }
        [HttpPut("MarkRead/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            //var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (userIdStr == null)
            //    return Unauthorized();

            int userId = int.Parse("3");

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
