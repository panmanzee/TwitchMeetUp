using testforproject.Models;

namespace testforproject.Authen.Services
{
    public interface IJwtService
    {
        public bool IsLogin { get; }
        public int? UserId { get; }

        public Task<User?> GetUser();
    }
}
