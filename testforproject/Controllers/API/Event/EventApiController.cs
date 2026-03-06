using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using testforproject.Authen;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Controllers.API.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventApiController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;

        public EventApiController(ApplicationDbContext db, IJwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinRequest request)
        {
            var userId = _jwtService.UserId;

            if (userId == null)
                return Unauthorized(new { message = "Not logged in" });

            var eventItem = await _db.Events
                .Include(e => e.Participants)
                .Include(e => e.Owner)
                .FirstOrDefaultAsync(e => e.Eid == request.EventId);

            if (eventItem == null)
                return NotFound(new { message = "Event not found" });

            if (eventItem.Owner.Uid == userId)
                return BadRequest(new { message = "You cannot join your own event" });

            if (eventItem.status == "closed")
                return BadRequest(new { message = "Event is closed" });

            if (eventItem.IsExpired || eventItem.EventStop < DateTime.Now || eventItem.status == "closed")
            {
                return BadRequest(new { message = "Registration for this event is closed or the event has already ended." });
            }
            if (eventItem.Participants.Any(u => u.Uid == userId))
                return BadRequest(new { message = "Already joined" });

            var user = await _db.Users.FindAsync(userId);

            eventItem.Participants.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Joined successfully" });
        }

        [HttpPost("unjoin")]
        public async Task<IActionResult> Unjoin([FromBody] JoinRequest request)
        {
            var userId = _jwtService.UserId;

            if (userId == null)
                return Unauthorized(new { message = "Not logged in" });

            var eventItem = await _db.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Eid == request.EventId);

            if (eventItem == null)
                return NotFound(new { message = "Event not found" });

            if (eventItem.EventStop < DateTime.Now || eventItem.status == "closed")
            {
                return BadRequest(new { message = "Cannot unjoin an event that is closed or has already ended." });
            }
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized(new { message = "User not found" });

            if (!eventItem.Participants.Any(u => u.Uid == userId))
                return BadRequest(new { message = "You have not joined this event" });

            eventItem.Participants.Remove(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Unjoined successfully" });
        }
    }

    public class JoinRequest
    {
        public int EventId { get; set; }
    }
}
