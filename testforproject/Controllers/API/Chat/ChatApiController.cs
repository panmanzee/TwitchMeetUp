using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography;
using System.Text;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;
using testforproject.Hubs;

namespace testforproject.Controllers.API.Chat
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatApiController(ApplicationDbContext db, IJwtService jwtService, IHubContext<ChatHub> hubContext)
        {
            _db = db;
            _jwtService = jwtService;
            _hubContext = hubContext;
        }

        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetMessages(int eventId)
        {
            var userId = _jwtService.UserId;
            if (userId == null) return Unauthorized();

            var ev = await _db.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Eid == eventId);

            if (ev == null) return NotFound("Event not found");

            bool isOwner = ev.OwnerId == userId;
            bool isParticipant = ev.Participants.Any(p => p.Uid == userId);

            if (!isOwner && !isParticipant)
            {
                return Forbid();
            }

            var messagesQuery = await _db.ChatMessages
                .Include(m => m.User)
                .Where(m => m.EventId == eventId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            var messages = messagesQuery.Select(m => new
                {
                    m.Id,
                    m.Message,
                    m.Timestamp,
                    SenderId = m.UserId,
                    // If the user requesting is the admin (Owner), show real username.
                    // Otherwise, hash the name (unless the sender is the user requesting).
                    SenderName = m.UserId == userId ? "You" :
                                 isOwner ? (m.User.Username ?? m.User.DisplayName ?? "Unknown") :
                                 HashUsername(m.User.Username ?? m.User.DisplayName ?? "Unknown", m.UserId)
                }).ToList();

            return Ok(messages);
        }

        [HttpPost("{eventId}")]
        public async Task<IActionResult> PostMessage(int eventId, [FromBody] ChatMessageDto dto)
        {
            var userId = _jwtService.UserId;
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Message)) return BadRequest("Message cannot be empty.");

            var ev = await _db.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Eid == eventId);

            if (ev == null) return NotFound();

            bool isOwner = ev.OwnerId == userId;
            bool isParticipant = ev.Participants.Any(p => p.Uid == userId);

            if (!isOwner && !isParticipant)
            {
                return Forbid();
            }

            var message = new ChatMessage
            {
                EventId = eventId,
                UserId = (int)userId,
                Message = dto.Message,
                Timestamp = DateTime.UtcNow
            };

            _db.ChatMessages.Add(message);
            await _db.SaveChangesAsync();

            // Notify all clients in the event group via SignalR WebSocket
            await _hubContext.Clients.Group($"Event_{eventId}").SendAsync("ReceiveNewMessage");

            return Ok(new { success = true });
        }

        private static string HashUsername(string username, int userId)
        {
            using (var sha256 = SHA256.Create())
            {
                // We hash the userId + a secret salt (for extra privacy so people can't map user IDs)
                // Just for this demo, a static string salt is used.
                string rawData = $"{userId}_ChatSecret_{username}";
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                // Return a friendlier shorter hash. e.g. User_a1b2c3
                return "User_" + builder.ToString().Substring(0, 6).ToUpper();
            }
        }
    }

    public class ChatMessageDto
    {
        public string Message { get; set; } = string.Empty;
    }
}
