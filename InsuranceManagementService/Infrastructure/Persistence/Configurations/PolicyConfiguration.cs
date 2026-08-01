using InsuranceManagementService.Domain.Entities;
using InsuranceManagementService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagementService.Infrastructure.Persistence.Configurations;

public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Number)
            .HasConversion(number => number.Value, value => PolicyNumber.FromValue(value))
            .HasColumnName("PolicyNumber")
            .HasMaxLength(32)
            .IsRequired();
        builder.HasIndex(p => p.Number).IsUnique();

        builder.Property(p => p.CustomerId).IsRequired();

        builder.Property(p => p.ProductType).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(p => p.BillingFrequency).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(p => p.Version).IsRequired();

        builder.Property(p => p.CancellationReason).HasMaxLength(500);

        builder.OwnsOne(p => p.Term, term =>
        {
            term.Property(t => t.Start).HasColumnName("TermStartUtc").IsRequired();
            term.Property(t => t.End).HasColumnName("TermEndUtc").IsRequired();
        });
        builder.Navigation(p => p.Term).IsRequired();

        builder.OwnsOne(p => p.Premium, premium =>
        {
            premium.Property(m => m.Amount).HasColumnName("PremiumAmount").HasColumnType("decimal(18,2)").IsRequired();
            premium.Property(m => m.Currency).HasColumnName("PremiumCurrency").HasMaxLength(3).IsRequired();
        });
        builder.Navigation(p => p.Premium).IsRequired();

        builder.HasMany(p => p.Coverages)
            .WithOne()
            .HasForeignKey(c => c.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Policy.Coverages))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Transactions)
            .WithOne()
            .HasForeignKey(t => t.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Policy.Transactions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
