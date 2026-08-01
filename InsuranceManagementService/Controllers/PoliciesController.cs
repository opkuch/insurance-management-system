using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Policies;
using InsuranceManagementService.Application.Policies.Dto;
using InsuranceManagementService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagementService.Controllers;

[ApiController]
[Produces("application/json")]
public sealed class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policies;

    public PoliciesController(IPolicyService policies) => _policies = policies;

    /// <summary>Issues a new policy to an existing, active customer.</summary>
    [HttpPost("api/customers/{customerId:guid}/policies")]
    [ProducesResponseType(typeof(PolicyDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PolicyDetailResponse>> Issue(
        Guid customerId,
        [FromBody] IssuePolicyRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await _policies.IssueAsync(customerId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = policy.Id }, policy);
    }

    /// <summary>Retrieves a policy with its coverages and full transaction history.</summary>
    [HttpGet("api/policies/{id:guid}")]
    [ProducesResponseType(typeof(PolicyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var policy = await _policies.GetByIdAsync(id, cancellationToken);
        return Ok(policy);
    }

    /// <summary>Lists policies, filterable by line of business, status, and customer.</summary>
    [HttpGet("api/policies")]
    [ProducesResponseType(typeof(PagedResult<PolicySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PolicySummaryResponse>>> List(
        [FromQuery] ProductType? productType,
        [FromQuery] PolicyStatus? status,
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _policies.ListAsync(
            productType,
            status,
            customerId,
            new PageRequest(page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists every policy belonging to a specific customer.</summary>
    [HttpGet("api/customers/{customerId:guid}/policies")]
    [ProducesResponseType(typeof(IReadOnlyList<PolicySummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PolicySummaryResponse>>> ListByCustomer(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var policies = await _policies.ListByCustomerAsync(customerId, cancellationToken);
        return Ok(policies);
    }

    /// <summary>Applies a mid-term change (endorsement) to a policy.</summary>
    [HttpPatch("api/policies/{id:guid}")]
    [ProducesResponseType(typeof(PolicyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PolicyDetailResponse>> Endorse(
        Guid id,
        [FromBody] EndorsePolicyRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await _policies.EndorseAsync(id, request, cancellationToken);
        return Ok(policy);
    }

    /// <summary>Cancels a policy before the end of its term.</summary>
    [HttpPost("api/policies/{id:guid}/cancel")]
    [ProducesResponseType(typeof(PolicyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PolicyDetailResponse>> Cancel(
        Guid id,
        [FromBody] CancelPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await _policies.CancelAsync(id, request, cancellationToken);
        return Ok(policy);
    }
}
