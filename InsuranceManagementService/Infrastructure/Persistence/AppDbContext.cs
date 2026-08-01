using InsuranceManagementService.Application.Abstractions;
using InsuranceManagementService.Domain.Common;
using InsuranceManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace InsuranceManagementService.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    private readonly IClock _clock;

    public AppDbContext(DbContextOptions<AppDbContext> options, IClock clock)
        : base(options)
    {
        _clock = clock;
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Policy> Policies => Set<Policy>();

    public DbSet<PolicyNumberSequence> PolicyNumberSequences => Set<PolicyNumberSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Entity Ids are assigned by the domain (client-generated), so EF must not
        // treat a set key as an already-existing row when a new child is added to a
        // tracked aggregate. Otherwise it would issue UPDATEs instead of INSERTs.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey is { Properties.Count: 1 } &&
                primaryKey.Properties[0] is { Name: "Id" } idProperty &&
                idProperty.ClrType == typeof(Guid))
            {
                idProperty.ValueGenerated = ValueGenerated.Never;
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampTimestamps();
        return base.SaveChanges();
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await action(cancellationToken);
            return;
        }

        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private void StampTimestamps()
    {
        var now = _clock.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.StampCreated(now);
                    break;
                case EntityState.Modified:
                    entry.Entity.StampUpdated(now);
                    break;
            }
        }
    }
}
