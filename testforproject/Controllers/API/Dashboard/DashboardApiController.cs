using Microsoft.AspNetCore.Mvc;
using testforproject.Authen;
using testforproject.Authen.Services;
using testforproject.Data;

namespace testforproject.Controllers.API.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardApiController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        public DashboardApiController(ApplicationDbContext db, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _db = db;
        }


        [HttpGet("LoadMoreEvents")]
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
                                owner = x.Owner,
                                location = x.Location,
                                //duration = x.Duration,
                                maxParticitpant = x.MaxParticitpant,
                                particitpant = x.Participants
                            })
                            .ToList();

            return Json(events);
        }
    }
}
