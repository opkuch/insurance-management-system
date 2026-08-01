using InsuranceManagementService.Application.Abstractions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagementService.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        => await _db.Customers.AddAsync(customer, cancellationToken);

    public async Task<Customer?> GetByIdAsync(Guid id, bool includePolicies = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Customers.AsQueryable();
        if (includePolicies)
        {
            query = query.Include(c => c.Policies).ThenInclude(p => p.Coverages);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default)
        => _db.Customers.AnyAsync(c => c.NationalId == nationalId, cancellationToken);

    public async Task<PagedResult<Customer>> ListAsync(
        string? search,
        CustomerStatus? status,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Customers.AsNoTracking();

        if (status is not null)
        {
            query = query.Where(c => c.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var pattern = $"%{term}%";
            query = query.Where(c =>
                EF.Functions.Like(c.FullName, pattern) ||
                EF.Functions.Like(c.NationalId, pattern) ||
                EF.Functions.Like(c.Email, pattern));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.FullName)
            .Skip(page.Skip)
            .Take(page.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<Customer>(items, page.NormalizedPage, page.NormalizedPageSize, total);
    }
}
