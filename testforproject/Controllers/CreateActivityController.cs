using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using testforproject.Data;
using testforproject.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace testforproject.Controllers
{
    [Authorize] 
    public class CreateActivityController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CreateActivityController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Create()
        {
            
            ViewBag.ExistingCategories = _db.Categories.ToList() ?? new List<Category>();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            string Name, string Location, int MaxParticitpant,
            List<int> SelectedCategoryIds, DateTime Expired_Date,
            string Decription, DateTime StartTime, DateTime closetime)
        {
            
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int currentUserId = int.Parse(userIdString);

            if (closetime <= StartTime)
            {
                ModelState.AddModelError("closetime", "Event Stop must be later than Event Start.");
            }
            if (Expired_Date <= closetime)
            {
                ModelState.AddModelError("Expired_Date", "Expired Date must be after Event Stop.");
            }
            if (MaxParticitpant < 1)
            {
                ModelState.AddModelError("MaxParticitpant", "Max Participants must be at least 1.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ExistingCategories = _db.Categories.ToList();
                return View(); // return form with errors
            }

            var defaultReq = new Requirements
            {
                Gender = "Any",
                Age = "Any",
                ParticipantScore = 0
            };
            _db.Requirements.Add(defaultReq);
            await _db.SaveChangesAsync(); 

            
            var newEvent = new Event
            {
                Name = Name,
                Location = Location,
                MaxParticitpant = MaxParticitpant,
                OwnerId = currentUserId, 
                ExpiredDate = Expired_Date,
                EventStart = StartTime == default ? DateTime.Now : StartTime,
                EventStop = closetime ,
                status = "open",
                Description = Decription ?? "",
                
                
            };

           
            if (SelectedCategoryIds != null)
            {
                foreach (var catId in SelectedCategoryIds)
                {
                    var category = await _db.Categories.FindAsync(catId);
                    if (category != null)
                    {
                        newEvent.Categories.Add(category);
                    }
                }
            }

            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }
    }
}