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

        private readonly IWebHostEnvironment _webHostEnvironment;
        public CreateActivityController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
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
            string Decription, DateTime StartTime, DateTime closetime, IFormFile? ImageFile)
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

            // 3. Image Upload Logic Start
            string savedImageUrl = null;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Find the wwwroot/images/events folder path
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "events");

                // Create the folder if it doesn't exist yet
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Generate a unique file name so old pictures don't get overwritten
                string safeFileName = Path.GetFileName(ImageFile.FileName)
                                          .Replace(" ", "_"); // replace spaces with underscores

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + safeFileName;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                // Copy the uploaded file to the server folder
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                // Set the URL path that will be saved in the database
                savedImageUrl = "/images/events/" + uniqueFileName;
            }
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

                ImageUrl = savedImageUrl
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

            return RedirectToAction("Show", "Dashboard");
        }
    }
}