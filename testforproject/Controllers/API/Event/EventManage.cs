using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Controllers.API.Event
{
    [ApiController]
    [Route("api/events")]
    public class EventManage : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly testforproject.Features.Notification.INotification _notiService;

        public EventManage(ApplicationDbContext context, IJwtService jwtService, testforproject.Features.Notification.INotification notiService)
        {
            _context = context;
            _jwtService = jwtService;
            _notiService = notiService;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events
                .Where(e => e.Eid == id)
                .Select(e => new
                {
                    e.Eid,
                    e.Name,
                    e.EventStart,
                    e.EventStop,
                    e.ExpiredDate,
                    e.MaxParticitpant,
                    e.status,
                    e.Description,
                    e.Location,
                    e.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (ev == null) return NotFound();
            return Ok(ev);
        }


        [HttpGet("{id}/participants")]
        public async Task<IActionResult> GetParticipants(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Eid == id);

            if (ev == null) return NotFound();

            bool isClosed = ev.status == "closed";

            var confirmedUserIds = await _context.ParticipantConfirmations
                .Where(c => c.EventId == id)
                .Select(c => c.UserId)
                .ToListAsync();

            var users = await _context.EventParticipants
                .Where(ep => ep.Eid == id)
                .Select(ep => new
                {
                    ep.Participant.Uid,
                    ep.Participant.Username,
                    ep.Participant.DisplayName,
                    ep.Participant.ProfilePictureSrc,
                    JoinedAt = ep.JoinedAt,
                    IsConfirmed = confirmedUserIds.Contains(ep.ParticitpantUid)
                })
                .ToListAsync();

            return Ok(new { isClosed, users });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var userId = _jwtService.UserId;
            if (userId == null) return Unauthorized(new { message = "You must be logged in to edit events." });

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            if (ev.OwnerId != userId) return Forbid(); // Check authorization (IDOR protection)

            if (ev.status == "closed")
                return BadRequest(new { message = "Cannot edit a closed event." });

            ev.EventStart = dto.EventStart;
            ev.EventStop = dto.EventStop;
            ev.ExpiredDate = dto.ExpiredDate;
            ev.MaxParticitpant = dto.MaxParticitpant;

            if (!TryValidateModel(ev))
                return ValidationProblem(ModelState);

            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPost("{id}/participants/confirm")]
        public async Task<IActionResult> ConfirmParticipants(int id, [FromBody] ConfirmParticipantsDto dto)
        {
            var userId = _jwtService.UserId;
            if (userId == null) return Unauthorized(new { message = "You must be logged in." });

            var ev = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Eid == id);

            if (ev == null) return NotFound();

            // Remove existing confirmations for this event
            var existing = _context.ParticipantConfirmations.Where(c => c.EventId == id);
            _context.ParticipantConfirmations.RemoveRange(existing);

            if (ev.OwnerId != userId) return Forbid(); // Check authorization (IDOR protection)

            if (ev.status == "closed")
                return BadRequest(new { message = "Cannot change participants on a closed event." });

            var selectedUsers = await _context.Users
                .Where(u => dto.UserIds.Contains(u.Uid))
                .ToListAsync();

            ev.Participants.Clear();
            foreach (var user in selectedUsers)
                ev.Participants.Add(user);
            // Add new confirmations
            foreach (var uId in dto.UserIds)
            {
                _context.ParticipantConfirmations.Add(new ParticipantConfirmation
                {
                    EventId = id,
                    UserId = uId
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"{dto.UserIds.Count} participants confirmed." });
        }


        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var userId = _jwtService.UserId;
            if (userId == null) return Unauthorized(new { message = "You must be logged in." });

            var ev = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Eid == id);

            if (ev == null) return NotFound();

            if (ev.OwnerId != userId) return Forbid(); // Check authorization (IDOR protection)

            ev.status = dto.Status;
            await _context.SaveChangesAsync();

            if (dto.Status == "closed")
            {
                var participants = ev.Participants.ToList();
                if (participants.Any())
                {
                    var title = "You are confirmed!";
                    var desc = $"Great news! The event '{ev.Name}' is now finalized and you are confirmed as a participant.";
                    var date = DateTime.Now.ToString("dd MMM yyyy HH:mm");
                    var href = $"http://localhost:5189/Event/EventDetails/{ev.Eid}#";

                    await _notiService.CreateNotification(title, desc, date, participants, href, ev.OwnerId);
                }
            }

            return Ok(new
            {
                message = $"Event status updated to '{dto.Status}'.",
                remainingParticipants = ev.Participants.Count
            });
        }
    }

    public class UpdateEventDto
    {
        public DateTime EventStart { get; set; }
        public DateTime EventStop { get; set; }
        public DateTimeOffset ExpiredDate { get; set; }
        public int MaxParticitpant { get; set; }
    }

    public class ConfirmParticipantsDto
    {
        public List<int> UserIds { get; set; } = new();
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}