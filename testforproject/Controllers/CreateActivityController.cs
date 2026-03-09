using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly testforproject.Features.Notification.INotification _notiService;

        public CreateActivityController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, testforproject.Features.Notification.INotification notiService)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _notiService = notiService;
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
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }
            int currentUserId = int.Parse(userIdString);
            if (StartTime < DateTime.UtcNow.AddHours(7))
            {
                ModelState.AddModelError("StartTime", "Event Start must not be in the past.");
            }
            if (closetime <= StartTime)
            {
                ModelState.AddModelError("closetime", "Event Stop must be later than Event Start.");
            }
            if (Expired_Date > StartTime)
            {
                ModelState.AddModelError("Expired_Date", "Registration end time (Expired Date) must be before the Event Start.");
            }
            if (MaxParticitpant < 1)
            {
                ModelState.AddModelError("MaxParticitpant", "Max Participants must be at least 1.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ExistingCategories = _db.Categories.ToList();
                return View();
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
                ExpiredDate = new DateTimeOffset(Expired_Date, TimeSpan.FromHours(7)),
                EventStart = StartTime == default ? DateTime.UtcNow.AddHours(7) : StartTime,
                EventStop = closetime,
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

            // Notify followers
            var currentUserWithFollowers = await _db.Users
                .Include(u => u.Follower)
                .FirstOrDefaultAsync(u => u.Uid == currentUserId);

            if (currentUserWithFollowers?.Follower != null && currentUserWithFollowers.Follower.Any())
            {
                var title = $"{currentUserWithFollowers.DisplayName} created a new event!";
                var desc = $"Check out \"{newEvent.Name}\" happening at {newEvent.Location}.";
                var date = DateTime.UtcNow.AddHours(7).ToString("dd MMM yyyy HH:mm");
                var href = $"Event/EventDetails/{newEvent.Eid}#";

                await _notiService.CreateNotification(title, desc, date, currentUserWithFollowers.Follower.ToList(), href, currentUserId);
            }

            return RedirectToAction("Show", "Dashboard");
        }
    }
}