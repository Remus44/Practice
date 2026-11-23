using Hangfire;

namespace BatchLab.Jobs;

public class DemoJob
{
    private readonly ILogger<DemoJob> _logger;

    public DemoJob(ILogger<DemoJob> logger)
    {
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Hangfire job started!");

        await Task.Delay(3000);

        _logger.LogInformation("Hangfire job finished!");
    }

    [AutomaticRetry(
        Attempts = 2,
        DelaysInSeconds = new[] { 2, 4 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public Task FailAsync()
    {
        _logger.LogWarning("Demo job: szándékos hiba!");

        return Task.FromException(
            new InvalidOperationException(
                "Simulated database connection failure"));
    }
}