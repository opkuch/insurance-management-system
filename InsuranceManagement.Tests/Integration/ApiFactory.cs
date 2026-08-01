using InsuranceManagementService.Application.Abstractions;
using InsuranceManagementService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InsuranceManagement.Tests.Integration;

/// <summary>
/// Boots the real API against an isolated in-memory SQLite database. Each factory
/// instance owns its own connection, so tests do not share state.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _keepAlive;
    private readonly string _connectionString = $"Data Source=file:insurance-{Guid.NewGuid():N}?mode=memory&cache=shared";
    private ControllableClock? _clock;

    /// <summary>Optional controllable clock for lifecycle sweep tests.</summary>
    public ControllableClock UseControllableClock(DateTime utcNow)
    {
        _clock = new ControllableClock(utcNow);
        return _clock;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var optionsDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions))
                .ToList();
            foreach (var descriptor in optionsDescriptors)
            {
                services.Remove(descriptor);
            }

            // Keep-alive connection so the shared in-memory database survives between scopes.
            // Separate connections per scope allow concurrent requests to participate in locking.
            _keepAlive = new SqliteConnection(_connectionString);
            _keepAlive.Open();
            using (var cmd = _keepAlive.CreateCommand())
            {
                cmd.CommandText = "PRAGMA busy_timeout = 5000;";
                cmd.ExecuteNonQuery();
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connectionString));

            if (_clock is not null)
            {
                services.RemoveAll<IClock>();
                services.AddSingleton<IClock>(_clock);
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAlive?.Dispose();
        }
    }
}

/// <summary>Test clock that can be advanced to exercise time-driven lifecycle transitions.</summary>
public sealed class ControllableClock : IClock
{
    public ControllableClock(DateTime utcNow) => UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public DateTime UtcNow { get; set; }
}
