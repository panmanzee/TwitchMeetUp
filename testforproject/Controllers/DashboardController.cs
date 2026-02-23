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

        
        public IActionResult Show()
        {
            var events = _db.Events
                            
                            .Include(e => e.Owner)
                            .OrderByDescending(e => e.Eid)
                            .Take(4)
                            .ToList();

            ViewBag.Categories = _db.Categories.ToList();
            return View(events);
        }

        
        [HttpGet]
        public IActionResult LoadMoreEvents(int skip)
        {
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
                                particitpant = x.Participants
                            })
                            .ToList();

            return Json(events);
        }
    }
}