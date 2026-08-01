using InsuranceManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagementService.Infrastructure.Persistence.Configurations;

public sealed class PolicyTransactionConfiguration : IEntityTypeConfiguration<PolicyTransaction>
{
    public void Configure(EntityTypeBuilder<PolicyTransaction> builder)
    {
        builder.ToTable("PolicyTransactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(t => t.EffectiveDateUtc).IsRequired();

        builder.Property(t => t.PreviousStatus).HasConversion<string>().HasMaxLength(16);

        builder.Property(t => t.NewStatus).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(t => t.Notes).HasMaxLength(500);

        builder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(120);

        builder.HasIndex(t => t.PolicyId);
    }
}
