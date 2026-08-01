using InsuranceManagementService.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InsuranceManagementService.Infrastructure.Persistence;

/// <summary>
/// Enables design-time tooling (dotnet ef migrations) to construct the context
/// without booting the full application host.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=insurance.db")
            .Options;

        return new AppDbContext(options, new SystemClock());
    }
}
