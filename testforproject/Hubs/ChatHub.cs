using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;

        public ChatHub(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task JoinEventGroup(string eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Event_{eventId}");
        }

        public async Task LeaveEventGroup(string eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Event_{eventId}");
        }

        public async Task SendMessage(int eventId, string messageText, int userId)
        {
            if (string.IsNullOrWhiteSpace(messageText)) return;

            var ev = await _db.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Eid == eventId);

            if (ev == null) return;

            bool isOwner = ev.OwnerId == userId;
            bool isParticipant = ev.Participants.Any(p => p.Uid == userId);

            if (!isOwner && !isParticipant) return;

            var message = new ChatMessage
            {
                EventId = eventId,
                UserId = userId,
                Message = messageText,
                Timestamp = DateTime.UtcNow
            };

            _db.ChatMessages.Add(message);
            await _db.SaveChangesAsync();

            // Broadcast that a new message is ready to be fetched
            await Clients.Group($"Event_{eventId}").SendAsync("ReceiveNewMessage");
        }
    }
}
