using HotelApp.BL.Interfaces;

namespace HotelApp.Web.BackgroundServices
{
    /// <summary>
    /// RN-009: every few minutes, cancels as "No Show" the reservations whose guest never checked in.
    /// An error in one run is logged and does not stop the next ones.
    /// </summary>
    public class NoShowBackgroundService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NoShowBackgroundService> _logger;

        public NoShowBackgroundService(IServiceScopeFactory scopeFactory, ILogger<NoShowBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            do
            {
                try
                {
                    // Services are scoped (one DbContext per run), so a new scope is created each time
                    using var scope = _scopeFactory.CreateScope();
                    var reservations = scope.ServiceProvider.GetRequiredService<IReservationService>();

                    var count = await reservations.ProcessNoShowsAsync();
                    if (count > 0)
                        _logger.LogInformation("{Count} reservation(s) marked as No Show.", count);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No-Show processing failed; it will be retried in {Interval}.", Interval);
                }
            }
            while (await WaitForNextTickAsync(timer, stoppingToken));
        }

        private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken stoppingToken)
        {
            try
            {
                return await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
