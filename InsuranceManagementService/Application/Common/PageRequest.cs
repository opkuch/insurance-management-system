namespace InsuranceManagementService.Application.Common;

/// <summary>
/// Normalized pagination parameters. Guards against invalid or abusive page sizes.
/// </summary>
public sealed record PageRequest(int Page = 1, int PageSize = 20)
{
    public const int MaxPageSize = 100;

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => PageSize is < 1 or > MaxPageSize ? 20 : PageSize;

    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;

    public int Take => NormalizedPageSize;
}
