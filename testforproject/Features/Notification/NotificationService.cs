using Microsoft.EntityFrameworkCore;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Features.Notification
{
    public class NotificationService : INotification

    {
        private readonly ApplicationDbContext _db;
        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CreateNotification(string Title, string Description, string Date, List<User> user, string? Href = null, int? TriggerUserUid = null)
        {
            for (int i = 0; i < user.Count; i++)
            {
                var Noti = new testforproject.Models.Notification
                {
                    Title = Title,
                    Description = Description,
                    Date = Date,
                    UserUid = user[i].Uid,
                    Href = Href,
                    TriggerUserUid = TriggerUserUid
                };
                await _db.Notifications.AddAsync(Noti);
                await _db.SaveChangesAsync();
            }

        }
    }
}
