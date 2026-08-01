namespace InsuranceManagementService.Application.Common;

/// <summary>
/// A single page of results together with the metadata needed to page through them.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
