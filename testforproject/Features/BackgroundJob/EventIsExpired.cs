using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            await CheckExpiredEvents();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CheckExpiredEvents()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var expiredEvents = db.Events
            .Where(e => e.ExpiredDate < DateTimeOffset.UtcNow
                     && e.status == "open")
            .ToList();

        foreach (var ev in expiredEvents)
        {
            ev.status = "closed";


            db.Notifications.Add(new Models.Notification
            {
                Title = "Test",
                UserUid = ev.OwnerId,
                Description = $" '{ev.Name}'  expired.",
                Date = (DateTimeOffset.UtcNow).ToString(),

            });
        }

        await db.SaveChangesAsync();
        _logger.LogInformation($" {expiredEvents.Count} ");
    }
}