using InsuranceManagementService.Domain.Entities;
using InsuranceManagementService.Domain.ValueObjects;

namespace InsuranceManagementService.Application.Abstractions;

/// <summary>
/// Produces the next unique, sequential policy number for a line of business and year.
/// The reserved sequence is persisted as part of the same unit of work as the policy.
/// </summary>
public interface IPolicyNumberGenerator
{
    Task<PolicyNumber> NextAsync(ProductType productType, int year, CancellationToken cancellationToken = default);
} 
