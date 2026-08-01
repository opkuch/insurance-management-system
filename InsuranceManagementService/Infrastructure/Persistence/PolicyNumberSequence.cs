using InsuranceManagementService.Domain.Entities;

namespace InsuranceManagementService.Infrastructure.Persistence;

/// <summary>
/// Persistence-only counter that hands out monotonically increasing sequence
/// values per line of business and year, backing policy number generation.
/// </summary>
public class PolicyNumberSequence
{
    private PolicyNumberSequence()
    {
    }

    private PolicyNumberSequence(ProductType productType, int year)
    {
        ProductType = productType;
        Year = year;
        NextValue = 1;
    }

    public ProductType ProductType { get; private set; }

    public int Year { get; private set; }

    public long NextValue { get; private set; }

    public static PolicyNumberSequence Start(ProductType productType, int year) => new(productType, year);

    /// <summary>Returns the current value and advances the counter.</summary>
    public long Reserve()
    {
        var reserved = NextValue;
        NextValue++;
        return reserved;
    }
}
