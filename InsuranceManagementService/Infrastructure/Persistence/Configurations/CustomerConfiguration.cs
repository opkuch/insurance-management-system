using InsuranceManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagementService.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName).IsRequired().HasMaxLength(200);

        builder.Property(c => c.NationalId).IsRequired().HasMaxLength(64);
        builder.HasIndex(c => c.NationalId).IsUnique();

        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);

        builder.Property(c => c.PhoneNumber).HasMaxLength(32);

        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.Line1).HasColumnName("AddressLine1").HasMaxLength(200);
            address.Property(a => a.Line2).HasColumnName("AddressLine2").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(120);
            address.Property(a => a.State).HasColumnName("AddressState").HasMaxLength(120);
            address.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(32);
            address.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(120);
        });

        builder.HasMany(c => c.Policies)
            .WithOne()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata.FindNavigation(nameof(Customer.Policies))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
