namespace InsuranceManagementService.Application.Abstractions;

/// <summary>
/// Commits all changes tracked within a single business operation atomically.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="action"/> inside a database transaction and commits on success.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}
