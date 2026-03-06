using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Claims;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;

        public ProfileController(ApplicationDbContext db ,IJwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        [Route("User/{name?}")]
        public async Task<IActionResult> Index(string name)
        {
            User? currentUser = await _jwtService.GetUser();
            var userId = _jwtService.UserId;

            
            if (userId != null)
            {
                currentUser = await _db.Users
                    .Include(u => u.Following)
                    .Include(u => u.OwningEvent)
                    .Include(u => u.ParticipatedEvent)
                    .FirstOrDefaultAsync(u => u.Uid == userId);
            }

            
            var queryUser = await _db.Users
                .Include(u => u.Follower)
                .Include(u => u.OwningEvent)
                .Include(u => u.ParticipatedEvent)
                .FirstOrDefaultAsync(u => u.Username == name);

            
            if (currentUser == null && name == null)
            {
                return NotFound();
            }

            if (currentUser != null && name == null)
            {
                ViewBag.user = currentUser.Username;
                return View(currentUser);
            }
            if ( queryUser == null)
            {
                return NotFound();
            }


            if (currentUser != null)
            {
                ViewBag.user = currentUser.Username;
                ViewBag.IsFollowing = currentUser.Following.Any(f => f.Uid == queryUser.Uid);
            }
            else
            {
                ViewBag.user = null;
                ViewBag.IsFollowing = false;
            }

           
            ViewBag.FollowerCount = queryUser.Follower?.Count ?? 0;

            return View(queryUser);
        }
        public async Task<IActionResult> Edit()
        {
            var userId = _jwtService.UserId;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Uid == userId);
            return View(user);
        }
    }
}