using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;
using testforproject.Services;
using testforproject.Authen.Services;
using System.Collections.Generic;

namespace testforproject.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly RecommendationService _recommendationService;
        private readonly IJwtService _jwtService;

        public DashboardController(ApplicationDbContext db, RecommendationService recommendationService, IJwtService jwtService)
        {
            _db = db;
            _recommendationService = recommendationService;
            _jwtService = jwtService;
        }


        public IActionResult Show(string searchQuery, string sortorder, string categoryFilter)
        {
            // --- Phase 5: Fetch Recommendations ---
            var userId = _jwtService.UserId; // this is likely int?
            ViewBag.IsLoggedIn = (userId != null && userId.Value != 0);

            if (ViewBag.IsLoggedIn)
            {
                var user = _db.Users.FirstOrDefault(u => u.Uid == userId!.Value);
                if (user != null)
                {
                    ViewBag.Username = user.Username;
                    ViewBag.ProfilePic = user.ProfilePictureSrc;
                    ViewBag.Uid = user.Uid;
                }
            }

            List<RecommendationScoreResult> recommendedEvents = new List<RecommendationScoreResult>();

            if (userId != null && userId.Value != 0) // if logged in
            {
                // Fetch top 5 personalized events for the logged-in user
                recommendedEvents = _recommendationService.GetRecommendationsForUser(userId.Value, 20);
            }
            else
            {
                // Guest / Not logged-in fallback: Fetch generic popular events
                recommendedEvents = _recommendationService.GetRecommendationsForUser(0, 20);
            }

            ViewBag.RecommendedEvents = recommendedEvents;
            // --------------------------------------

            var query = _db.Events
                           .Include(e => e.Owner)
                           .Include(e => e.Categories)
                           .AsQueryable();

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                query = query.Where(e => e.Categories.Any(c => c.Name == categoryFilter));
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(e =>
                    e.Name.Contains(searchQuery) ||
                    e.Categories.Any(c => c.Name.Contains(searchQuery))
                );
            }

            switch (sortorder)
            {
                case "name_asc":
                    query = query.OrderBy(e => e.Name);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(e => e.Name);
                    break;
                default:
                    query = query.OrderByDescending(e => e.Eid);
                    break;
            }

            var events = query.Take(4).ToList();

            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.IsLoggedIn = _jwtService.UserId != null;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortOrder = sortorder;

            return View(events);
        }

        [HttpGet]
        public IActionResult SearchEventsAJAX(string searchQuery, string sortorder, string categoryFilter)
        {
            var query = _db.Events
                           .Include(e => e.Owner)
                           .Include(e => e.Categories)
                           .AsQueryable();

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                query = query.Where(e => e.Categories.Any(c => c.Name == categoryFilter));
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(e =>
                    e.Name.Contains(searchQuery) ||
                    e.Categories.Any(c => c.Name.Contains(searchQuery))
                );
            }


            switch (sortorder)
            {
                case "name_asc":
                    query = query.OrderBy(e => e.Name);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(e => e.Name);
                    break;
                default:
                    query = query.OrderByDescending(e => e.Eid);
                    break;
            }


            var events = query.Take(4).ToList();


            return PartialView("_EventGridPartial", events);
        }

        [HttpGet]
        public IActionResult LoadMoreEvents(int skip, string searchQuery, string categoryFilter)
        {
            var query = _db.Events.Include(e => e.Owner).Include(e => e.Categories).AsQueryable();

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                query = query.Where(e => e.Categories.Any(c => c.Name == categoryFilter));
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(e =>
                    e.Name.Contains(searchQuery) ||
                    e.Categories.Any(c => c.Name.Contains(searchQuery))
                );
            }
            var events = query
                            .OrderByDescending(x => x.Eid)
                            .Skip(skip)
                            .Take(4)
                            .Select(x => new
                            {
                                eid = x.Eid,
                                name = x.Name,
                                owner = x.Owner.Username,
                                location = x.Location,
                                Expire = x.ExpiredDate,
                                maxParticitpant = x.MaxParticitpant,
                                particitpant = x.Participants,
                                imageUrl = x.ImageUrl
                            })
                            .ToList();

            return Json(events);
        }
    }
}