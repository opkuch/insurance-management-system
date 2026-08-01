using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagementService.Infrastructure.Persistence.Configurations;

public sealed class PolicyNumberSequenceConfiguration : IEntityTypeConfiguration<PolicyNumberSequence>
{
    public void Configure(EntityTypeBuilder<PolicyNumberSequence> builder)
    {
        builder.ToTable("PolicyNumberSequences");

        builder.HasKey(s => new { s.ProductType, s.Year });

        builder.Property(s => s.ProductType).HasConversion<string>().HasMaxLength(16);

        builder.Property(s => s.NextValue).IsRequired();
    }
}
