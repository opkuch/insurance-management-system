using InsuranceManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagementService.Infrastructure.Persistence.Configurations;

public sealed class CoverageConfiguration : IEntityTypeConfiguration<Coverage>
{
    public void Configure(EntityTypeBuilder<Coverage> builder)
    {
        builder.ToTable("Coverages");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type).IsRequired().HasMaxLength(120);

        builder.OwnsOne(c => c.Limit, limit =>
        {
            limit.Property(m => m.Amount).HasColumnName("LimitAmount").HasColumnType("decimal(18,2)").IsRequired();
            limit.Property(m => m.Currency).HasColumnName("LimitCurrency").HasMaxLength(3).IsRequired();
        });
        builder.Navigation(c => c.Limit).IsRequired();

        builder.OwnsOne(c => c.Deductible, deductible =>
        {
            deductible.Property(m => m.Amount).HasColumnName("DeductibleAmount").HasColumnType("decimal(18,2)").IsRequired();
            deductible.Property(m => m.Currency).HasColumnName("DeductibleCurrency").HasMaxLength(3).IsRequired();
        });
        builder.Navigation(c => c.Deductible).IsRequired();
    }
}
