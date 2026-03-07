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


            if (eventItem.Participants.Any(u => u.Uid == userId))
                return BadRequest(new { message = "Already joined" });

            var user = await _db.Users.FindAsync(userId);

            eventItem.Participants.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Joined successfully" });
        }
    }

    public class JoinRequest
    {
        public int EventId { get; set; }
    }
}
