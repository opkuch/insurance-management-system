namespace InsuranceManagementService.Domain.Entities;

/// <summary>
/// Lifecycle state of a policy. Transitions are enforced by the Policy aggregate.
/// </summary>
public enum PolicyStatus
{
    /// <summary>Bound contract whose coverage term has not yet started.</summary>
    Issued = 1,

    /// <summary>Coverage is currently in force.</summary>
    Active = 2,

    /// <summary>Terminated before the natural end of its term.</summary>
    Cancelled = 3,

    /// <summary>Term ended without renewal.</summary>
    Expired = 4,
}

/// <summary>
/// Kind of lifecycle event recorded in a policy's audit log.
/// </summary>
public enum PolicyTransactionType
{
    Issued = 1,
    Endorsed = 2,
    Cancelled = 3,
    Expired = 4,
}

/// <summary>
/// How often the premium is billed over the policy term.
/// </summary>
public enum BillingFrequency
{
    Monthly = 1,
    Quarterly = 2,
    SemiAnnual = 3,
    Annual = 4,
}

/// <summary>
/// Line of business a policy belongs to.
/// </summary>
public enum ProductType
{
    Auto = 1,
    Health = 2,
    Life = 3,
}
