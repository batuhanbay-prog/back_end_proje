using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Product.Infrastructure.Configuration;

/// <summary>
/// EF Core Fluent API ile Product entity konfigürasyonu.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product.Domain.Entities.Product>
{
    public void Configure(EntityTypeBuilder<Product.Domain.Entities.Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Category).HasMaxLength(100);
        builder.Property(p => p.IsActive).HasDefaultValue(true);
    }
}
