using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Authen.Services
{
    public class JwtService:IJwtService
    {
        private readonly IHttpContextAccessor _httpContextAcessor;
        private readonly ApplicationDbContext _db;
        private User? _user;
        public JwtService(IHttpContextAccessor httpContextAcessor, ApplicationDbContext db)
        {
            _httpContextAcessor = httpContextAcessor;
            _db = db;
        }
        public int? UserId
        {
            get 
            {
                if (_httpContextAcessor.HttpContext?.User?.Identity?.IsAuthenticated != true) { return null; }
                    
                var userId = _httpContextAcessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(userId, out var id) ? id : null;
            }
        }
       
        public async Task<User?> GetUser()
        {
            
            if (_user != null) {
                return _user;
            }
            if (UserId == null) {
                return null; 
            }
            _user = await _db.Users.FirstOrDefaultAsync(u => u.Uid == UserId);
            return _user;
            
        }
        //mai chache because noi
        // chai UserId tan user because mai tong query database 
        public bool IsLogin => UserId != null;

    }
}
