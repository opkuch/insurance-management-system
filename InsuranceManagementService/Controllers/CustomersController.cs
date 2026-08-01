using InsuranceManagementService.Application.Common;
using InsuranceManagementService.Application.Customers;
using InsuranceManagementService.Application.Customers.Dto;
using InsuranceManagementService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagementService.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;

    public CustomersController(ICustomerService customers) => _customers = customers;

    /// <summary>Onboards a new customer.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    /// <summary>Retrieves a single customer with a summary of their policies.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customers.GetByIdAsync(id, cancellationToken);
        return Ok(customer);
    }

    /// <summary>Lists customers, optionally filtered by status or a free-text search.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CustomerListItemResponse>>> List(
        [FromQuery] string? search,
        [FromQuery] CustomerStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _customers.ListAsync(search, status, new PageRequest(page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Updates a customer's name and contact details.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> Update(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.UpdateAsync(id, request, cancellationToken);
        return Ok(customer);
    }

    /// <summary>Marks a customer as active so new policies can be issued.</summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CustomerResponse>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customers.ActivateAsync(id, cancellationToken);
        return Ok(customer);
    }

    /// <summary>Marks a customer as inactive; new policy issuance is blocked.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CustomerResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customers.DeactivateAsync(id, cancellationToken);
        return Ok(customer);
    }
}
