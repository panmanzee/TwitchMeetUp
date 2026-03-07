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
        private readonly testforproject.Features.Notification.INotification _notiService;

        public EventApiController(ApplicationDbContext db, IJwtService jwtService, testforproject.Features.Notification.INotification notiService)
        {
            _db = db;
            _jwtService = jwtService;
            _notiService = notiService;
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

            if (eventItem.Owner?.Uid == userId)
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

            _db.EventParticipants.Add(new EventParticipant
            {
                Eid = eventItem.Eid,
                ParticitpantUid = user.Uid,
                JoinedAt = DateTime.Now
            });

            await _db.SaveChangesAsync();

            // Notify event owner
            if (eventItem.Owner != null)
            {
                var title = "Someone joined your event!";
                var desc = $"{user.DisplayName ?? user.Username} has joined \"{eventItem.Name}\".";
                var date = DateTime.Now.ToString("dd MMM yyyy HH:mm");
                var href = $"http://localhost:5189/Profile/ManageEvent/{eventItem.Eid}#";

                await _notiService.CreateNotification(title, desc, date, new List<User> { eventItem.Owner }, href, user.Uid);
            }

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

            // Notify event owner
            // Need to fetch owner as it might not be included in unjoin initial query
            var eventWithOwner = await _db.Events.Include(e => e.Owner).FirstOrDefaultAsync(e => e.Eid == request.EventId);
            if (eventWithOwner?.Owner != null)
            {
                var title = "Someone left your event";
                var desc = $"{user.DisplayName ?? user.Username} has unjoined \"{eventWithOwner.Name}\".";
                var date = DateTime.Now.ToString("dd MMM yyyy HH:mm");
                var href = $"http://localhost:5189/Profile/ManageEvent/{eventWithOwner.Eid}#";

                await _notiService.CreateNotification(title, desc, date, new List<User> { eventWithOwner.Owner }, href, user.Uid);
            }

            return Ok(new { message = "Unjoined successfully" });
        }
    }

    public class JoinRequest
    {
        public int EventId { get; set; }
    }
}
