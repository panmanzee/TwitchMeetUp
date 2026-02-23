
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // สำคัญ: เพื่อให้ใช้ .Include() ได้
using System.Diagnostics;
using System.Security.Claims;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        public HomeController(ApplicationDbContext db, IJwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        
        public IActionResult Dashboard()
        {

            var events = _db.Events
              
              .Include(e => e.Owner)  
              .OrderByDescending(e => e.Eid)
              .Take(4)
              .ToList();
            return View(events);
        }

        public IActionResult Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _db.Users.FirstOrDefault(u => u.Uid == int.Parse(userId));
            return View(user);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult CreateActivity()
        {
            return View();
        }

       
        //[HttpPost]
        //public async Task<IActionResult> CreateActivity(string Name, string Location, int MaxParticitpant, string Catagories, string Duration)
        //{
        //    var defaultReq = new Requirements { Gender = "Any", Age = "Any", ParticipantScore = 0 };
        //    _db.Requirements.Add(defaultReq);
        //    await _db.SaveChangesAsync();
        //    var userId = _jwtService.UserId ?? 0; 

        //    if (userId == 0)
        //    {
        //        return Unauthorized();
        //    }
        //    var User = await _db.Users.FirstOrDefaultAsync(u => u.Uid == userId);
        //    var newEvent = new Event
        //    {
        //        Name = Name,
        //        Location = Location,
        //        MaxParticitpant = MaxParticitpant,
        //        OwnerId = userId,
        //        Duration = Duration,
        //        Catagories = Catagories.Split(',').ToList(), 
        //        Particitpant = new List<int>(),
        //        RequirementsId = defaultReq.RequirementsId
        //    };

        //    _db.Events.Add(newEvent);
        //    await _db.SaveChangesAsync();

        //    return RedirectToAction("Dashboard"); 
        //}

        
    }
}