namespace InsuranceManagementService.Application.Policies;

/// <summary>
/// Periodically advances time-driven policy states (activation and expiry) so
/// that statuses stay accurate even without a read touching each policy.
/// </summary>
public sealed class PolicyLifecycleWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PolicyLifecycleWorker> _logger;

    public PolicyLifecycleWorker(IServiceScopeFactory scopeFactory, ILogger<PolicyLifecycleWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            await SweepAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var policies = scope.ServiceProvider.GetRequiredService<IPolicyService>();
            var changed = await policies.RunLifecycleSweepAsync(cancellationToken);

            if (changed > 0)
            {
                _logger.LogInformation("Policy lifecycle sweep updated {Count} policy(ies).", changed);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutting down; nothing to do.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Policy lifecycle sweep failed.");
        }
    }
}
