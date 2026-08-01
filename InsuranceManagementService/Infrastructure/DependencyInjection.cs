using InsuranceManagementService.Application.Abstractions;
using InsuranceManagementService.Infrastructure.Persistence;
using InsuranceManagementService.Infrastructure.Persistence.Repositories;
using InsuranceManagementService.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagementService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=insurance.db";

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IPolicyNumberGenerator, PolicyNumberGenerator>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
