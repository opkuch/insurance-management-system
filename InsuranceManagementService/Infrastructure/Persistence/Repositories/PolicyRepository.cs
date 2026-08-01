using InsuranceManagementService.Application.Abstractions;
using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagementService.Infrastructure.Persistence.Repositories;

public sealed class PolicyRepository : IPolicyRepository
{
    private readonly AppDbContext _db;

    public PolicyRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
        => await _db.Policies.AddAsync(policy, cancellationToken);

    public Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Policies
            .Include(p => p.Coverages)
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResult<Policy>> ListAsync(
        ProductType? productType,
        PolicyStatus? status,
        Guid? customerId,
        DateTime nowUtc,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Policies.AsNoTracking().Include(p => p.Coverages).AsQueryable();

        if (productType is not null)
        {
            query = query.Where(p => p.ProductType == productType);
        }

        if (status is not null)
        {
            query = status switch
            {
                PolicyStatus.Cancelled => query.Where(p => p.Status == PolicyStatus.Cancelled),
                PolicyStatus.Expired => query.Where(p =>
                    p.Status != PolicyStatus.Cancelled && p.Term.End <= nowUtc),
                PolicyStatus.Active => query.Where(p =>
                    p.Status != PolicyStatus.Cancelled &&
                    p.Term.Start <= nowUtc &&
                    p.Term.End > nowUtc),
                PolicyStatus.Issued => query.Where(p =>
                    p.Status != PolicyStatus.Cancelled && p.Term.Start > nowUtc),
                _ => query,
            };
        }

        if (customerId is not null)
        {
            query = query.Where(p => p.CustomerId == customerId);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip(page.Skip)
            .Take(page.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<Policy>(items, page.NormalizedPage, page.NormalizedPageSize, total);
    }

    public async Task<IReadOnlyList<Policy>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await _db.Policies
            .AsNoTracking()
            .Include(p => p.Coverages)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Policy>> ListPendingLifecycleAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
        => await _db.Policies
            .Include(p => p.Transactions)
            .Where(p =>
                (p.Status == PolicyStatus.Issued && p.Term.Start <= nowUtc) ||
                (p.Status == PolicyStatus.Active && p.Term.End <= nowUtc))
            .ToListAsync(cancellationToken);
}
