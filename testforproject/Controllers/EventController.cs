using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using testforproject.Authen.Services;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly ILogger<EventController> _logger;
        public EventController(ApplicationDbContext db, IJwtService jwtService, ILogger<EventController> logger)
        {
            _db = db;
            _jwtService = jwtService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            var userId = _jwtService.UserId;
            if (userId == null)
            {
                return Redirect("Home/dashboard");
            }
            return View();
        }
        public IActionResult createEvent()
        {
            var userId = _jwtService.UserId;
            if (userId == null)
            {
                return Redirect("Home/dashboard");
            }
            return View();
        }
        public async Task<IActionResult> EventDetails(int id)
        {
            var eventDetail = await _db.Events
                .Include(e => e.Owner)
                .Include(e => e.Participants)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.Eid == id);

            if (eventDetail == null)
                return NotFound();


            var userId = _jwtService.UserId;
            var user = _db.Users.Find(userId);
            ViewBag.IsLoggedIn = userId != null;
            ViewBag.UserId = userId;
            ViewBag.IsJoined = userId != null &&
                               eventDetail.Participants.Any(u => u.Uid == userId);
            ViewBag.UserId = userId;
            ViewBag.ProfilePic = user?.ProfilePictureSrc;

            //attnedance part
            var attendRecords = await _db.AttendanceRecords
                .Where(a => a.EventId == id)
                .ToListAsync();

            var attendUserIds = attendRecords.Select(a => a.UserId).ToList();

            var attendUsers = await _db.Users
                .Where(u => attendUserIds.Contains(u.Uid))
                .ToListAsync();

            ViewBag.AttendCount = attendRecords.Count;
            ViewBag.AttendList = attendUsers;
            ViewBag.AttendTimes = attendRecords.ToDictionary(a => a.UserId, a => a.AttendedAt);
            //attnedance part


            return View(eventDetail);
        }

        [Route("Event/ManageEvent/{id}")]
        public async Task<IActionResult> ManageEvent(int id)
        {

            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            if (ev.OwnerId != _jwtService.UserId)
            {
                return NotFound();
            }
            return View(ev);
        }

        public IActionResult ServerTime()
        {
            return Content($@"
                Server UTC: {DateTime.UtcNow}
                Server Local: {DateTime.Now}
                DateTimeOffset Now: {DateTimeOffset.Now}
                ");
        }

        public async Task<IActionResult> QRCode(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            //var debugUrl = $"{Request.Scheme}://{Request.Host}";
            //return Content(debugUrl);

            //make URL
            var scanUrl = $"https://panmanzeeweb-hvhqg6fzcufmbnhx.japaneast-01.azurewebsites.net/Event/ScanAttend?eventId={id}";

            //Generate QR
            var qrBytes = await Task.Run(() =>
            {
                using var qrGenerator = new QRCodeGenerator();
                var qrData = qrGenerator.CreateQrCode(scanUrl, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrData);
                return qrCode.GetGraphic(10);
            });

            return File(qrBytes, "image/png");
        }

        public async Task<IActionResult> ScanAttend(int eventId)
        {
            var userId = _jwtService.UserId;
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var ev = await _db.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Eid == eventId);

            if (ev == null) return NotFound();
            //if (ev.ComputedStatus != "ongoing")
            //    return BadRequest("Event is not ongoing");

            if (userId == null)
            {
                var returnUrl = Uri.EscapeDataString(
                    Url.Action("ScanAttend", "Event", new { eventId }) ?? "/"
                );
                return Redirect($"/Account/Login?returnUrl={returnUrl}");
            }

            var isParticipant = ev.Participants.Any(u => u.Uid == userId);
            if (!isParticipant)
                return BadRequest("You are not a participant");

            //Check Scan
            var alreadyScanned = await _db.AttendanceRecords
                .AnyAsync(a => a.EventId == eventId && a.UserId == userId);
            if (alreadyScanned)
                return Content(@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta name='viewport' content='width=device-width, initial-scale=1'>
                        <meta http-equiv='refresh' content='3;url=/Dashboard/Show' />
                    </head>
                    <body style='background:#08080a; display:flex; align-items:center; justify-content:center; min-height:100vh; font-family:sans-serif; margin:0;'>
                        <div style='background:#111114; border:1px solid #2a2a30; border-radius:20px; padding:40px; text-align:center;'>
                            <div style='color:#4ade80; font-size:20px; font-weight:700; margin-top:12px;'>Already Attendancediv>
                            <div style='color:#8b8a96; font-size:14px; margin-top:8px;'>Redirecting to dashboard in 3 seconds...</div>
                        </div>
                    </body>
                    </html>", "text/html");

            _db.AttendanceRecords.Add(new AttendanceRecord
            {
                EventId = eventId,
                UserId = userId.Value
            });
            await _db.SaveChangesAsync();

            return Content(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <meta http-equiv='refresh' content='3;url=/Dashboard/Show' />
                </head>
                <body style='background:#08080a; display:flex; align-items:center; justify-content:center; min-height:100vh; font-family:sans-serif; margin:0;'>
                    <div style='background:#111114; border:1px solid #2a2a30; border-radius:20px; padding:40px; text-align:center;'>
                        <div style='color:#4ade80; font-size:20px; font-weight:700; margin-top:12px;'>Attendance Confirmed!</div>
                        <div style='color:#8b8a96; font-size:14px; margin-top:8px;'>Redirecting to dashboard in 3 seconds...</div>
                    </div>
                </body>
                </html>", "text/html");
        }
        
        
        [HttpGet("Event/EventScore/{id}")]
        public async Task<IActionResult> EventScore(int id)
        {
            var userId = _jwtService.UserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            var ev = await _db.Events.Include(e => e.Participants)
                                     .FirstOrDefaultAsync(e => e.Eid == id);
            
            if (ev == null) return NotFound();

            // Check if user is a participant
            if (!ev.Participants.Any(u => u.Uid == userId))
            {
                return Forbid();
            }

            // Check if user already scored
            var existingScore = await _db.EventScores.FirstOrDefaultAsync(s => s.EventId == id && s.UserId == userId);
            if (existingScore != null)
            {
                ViewBag.Message = "You have already rated this event.";
            }

            ViewBag.EventId = ev.Eid;
            ViewBag.EventName = ev.Name;

            return View(ev);
        }

        [HttpPost("Event/SubmitEventScore")]
        public async Task<IActionResult> SubmitEventScore(int eventId, int score, string comment)
        {
            var userId = _jwtService.UserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            var ev = await _db.Events.Include(e => e.Owner).FirstOrDefaultAsync(e => e.Eid == eventId);
            if (ev == null) return NotFound();

            var existingScore = await _db.EventScores.FirstOrDefaultAsync(s => s.EventId == eventId && s.UserId == userId);
            if (existingScore != null)
            {
                return BadRequest("You have already rated this event.");
            }

            var eventScore = new testforproject.Models.EventScore
            {
                EventId = eventId,
                UserId = (int)userId,
                Score = score,
                Comment = comment
            };

            _db.EventScores.Add(eventScore);
            await _db.SaveChangesAsync();

            // Recalculate HostScore
            var allHostScores = await _db.EventScores
                .Where(s => s.Event.OwnerId == ev.OwnerId)
                .AverageAsync(s => (double?)s.Score) ?? 0;

            var host = await _db.Users.FindAsync(ev.OwnerId);
            if (host != null)
            {
                host.HostScore = (int)Math.Round(allHostScores);
                _db.Users.Update(host);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("EventDetails", new { id = eventId });
        }
    }
}
