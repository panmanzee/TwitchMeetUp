using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using testforproject.Models;
using testforproject.Data;


namespace testforproject.Features.BackgroundJob;
public class EventIsExpired : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventIsExpired> _logger;

    public EventIsExpired(IServiceScopeFactory scopeFactory, ILogger<EventIsExpired> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // คำนวณเวลาที่เหลือจนกว่าจะถึงวินาทีที่ 00 ของนาทีถัดไป
            var now = DateTimeOffset.UtcNow;
            var nextMinute = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset).AddMinutes(1);
            var delay = nextMinute - now;

            // รอจนกว่าจะถึงเวลาเริ่มนาทีใหม่ (วินาทีที่ 00)
            await Task.Delay(delay, stoppingToken);

            // เมื่อถึงเวลา ค่อยรันโค้ดเช็ค
            if (!stoppingToken.IsCancellationRequested)
            {
                await CheckExpiredEvents();
            }
        }
    }

    private async Task CheckExpiredEvents()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notiService = scope.ServiceProvider.GetRequiredService<testforproject.Features.Notification.INotification>();

        var now = DateTimeOffset.UtcNow;

        // 1. Check Expired Events
        var expiredEvents = await db.Events
            .Include(e => e.Owner)
            .Where(e => e.ExpiredDate < now && e.status == "open")
            .ToListAsync();

        foreach (var ev in expiredEvents)
        {
            ev.status = "closed";

            // 1. Get existing manual confirmations
            var existingConfirmedIds = await db.ParticipantConfirmations
                .Where(c => c.EventId == ev.Eid)
                .Select(c => c.UserId)
                .ToListAsync();

            int slotsLeft = ev.MaxParticitpant - existingConfirmedIds.Count;

            List<User> newlyConfirmed = new List<User>();

            if (slotsLeft > 0)
            {
                // 2. Fetch unconfirmed participants sorted by join time
                var fcfsParticipants = await db.EventParticipants
                    .Include(ep => ep.Participant)
                    .Where(ep => ep.Eid == ev.Eid && !existingConfirmedIds.Contains(ep.ParticitpantUid))
                    .OrderBy(ep => ep.JoinedAt)
                    .Take(slotsLeft)
                    .Select(ep => ep.Participant)
                    .ToListAsync();

                if (fcfsParticipants.Any())
                {
                    foreach (var p in fcfsParticipants)
                    {
                        db.ParticipantConfirmations.Add(new ParticipantConfirmation
                        {
                            EventId = ev.Eid,
                            UserId = p.Uid
                        });
                        newlyConfirmed.Add(p);
                    }
                }
            }

            if (newlyConfirmed.Any() || existingConfirmedIds.Any())
            {
                // Save so we can fetch all confirmed users including new ones if we use a fresh query, 
                // or just combine the lists. Let's save.
                await db.SaveChangesAsync();

                // 4. Notify ALL confirmed participants (Manual + FCFS)
                var allConfirmedUsers = await db.ParticipantConfirmations
                    .Where(c => c.EventId == ev.Eid)
                    .Select(c => c.Participant)
                    .ToListAsync();

                if (allConfirmedUsers.Any())
                {
                    var notiTitle = "You are confirmed!";
                    var notiDesc = $"Great news! You've been confirmed for the event '{ev.Name}'.";
                    var notiDate = now.ToString("dd MMM yyyy HH:mm");
                    var notiHref = $"http://localhost:5189/Event/EventDetails/{ev.Eid}#";
                    await notiService.CreateNotification(notiTitle, notiDesc, notiDate, allConfirmedUsers, notiHref, null);
                }
            }

            if (ev.Owner != null)
            {
                var title = "Event Registration Closed";
                var desc = $"The registration for '{ev.Name}' has expired. {existingConfirmedIds.Count} were manual, and {newlyConfirmed.Count} were auto-confirmed.";
                var date = now.ToString("dd MMM yyyy HH:mm");
                var href = $"http://localhost:5189/Event/EventDetails/{ev.Eid}#";
                await notiService.CreateNotification(title, desc, date, new List<User> { ev.Owner }, href, null);
            }
        }

        // 2. Check Started Events
        var minuteAgo = now.AddMinutes(-1.5);
        var startedEvents = await db.Events
            .Include(e => e.Participants)
            .Where(e => e.EventStart <= now && e.EventStart > minuteAgo && e.status == "open")
            .ToListAsync();

        foreach (var ev in startedEvents)
        {
            if (ev.Participants != null && ev.Participants.Any())
            {
                var title = "Event Started!";
                var desc = $"The event '{ev.Name}' you joined has just started.";
                var date = now.ToString("dd MMM yyyy HH:mm");
                var href = $"http://localhost:5189/Event/EventDetails/{ev.Eid}#";
                await notiService.CreateNotification(title, desc, date, ev.Participants.ToList(), href, ev.OwnerId);
            }
        }

        // 3. Check Ended Events
        var endedEvents = await db.Events
             .Include(e => e.Participants)
             .Where(e => e.EventStop <= now && e.EventStop > minuteAgo && e.status == "open")
             .ToListAsync();

        foreach (var ev in endedEvents)
        {
            ev.status = "closed";
            if (ev.Participants != null && ev.Participants.Any())
            {
                var title = "Event Ended. Please Score!";
                var desc = $"The event '{ev.Name}' has concluded. Please leave a score and review.";
                var date = now.ToString("dd MMM yyyy HH:mm");
                var href = $"http://localhost:5189/Event/EventDetails/{ev.Eid}#score";
                await notiService.CreateNotification(title, desc, date, ev.Participants.ToList(), href, ev.OwnerId);
            }
        }

        await db.SaveChangesAsync();
        _logger.LogInformation($"Processed {expiredEvents.Count} expired, {startedEvents.Count} started, {endedEvents.Count} ended events.");
    }
}