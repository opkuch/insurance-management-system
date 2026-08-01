using InsuranceManagementService.Application.Customers;
using InsuranceManagementService.Application.Policies;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceManagementService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPolicyService, PolicyService>();

        return services;
    }
}
