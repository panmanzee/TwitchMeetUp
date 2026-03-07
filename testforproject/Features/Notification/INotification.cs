using testforproject.Models;

namespace testforproject.Features.Notification
{
    public interface INotification
    {
        public Task CreateNotification(string Title, string Description, string Date, List<User> user, string? Href = null, int? TriggerUserUid = null);

    }
}
