using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        
        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        
public IActionResult Show(string searchQuery,string sortorder,string categoryFilter)
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

            ViewBag.Categories = _db.Categories.ToList();
            
            
            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortOrder = sortorder; 

            return View(events);
        }
        [HttpGet]
        public IActionResult SearchEventsAJAX(string searchQuery, string sortorder,string categoryFilter)
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
        public IActionResult LoadMoreEvents(int skip, string searchQuery,string categoryFilter)
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
            var events = _db.Events
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