using InsuranceManagementService.Application.Abstractions;
using InsuranceManagementService.Domain.Entities;
using InsuranceManagementService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagementService.Infrastructure.Persistence;

public sealed class PolicyNumberGenerator : IPolicyNumberGenerator
{
    private readonly AppDbContext _db;

    public PolicyNumberGenerator(AppDbContext db) => _db = db;

    public async Task<PolicyNumber> NextAsync(ProductType productType, int year, CancellationToken cancellationToken = default)
    {
        // Reserve under the ambient transaction (Issue wraps this) so concurrent
        // issuers serialize on the sequence row. Unique index on Policy.Number is the backstop.
        var sequence = await _db.PolicyNumberSequences
            .FirstOrDefaultAsync(s => s.ProductType == productType && s.Year == year, cancellationToken);

        if (sequence is null)
        {
            sequence = PolicyNumberSequence.Start(productType, year);
            await _db.PolicyNumberSequences.AddAsync(sequence, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var value = sequence.Reserve();
        await _db.SaveChangesAsync(cancellationToken);

        return PolicyNumber.Create(productType, year, value);
    }
}
