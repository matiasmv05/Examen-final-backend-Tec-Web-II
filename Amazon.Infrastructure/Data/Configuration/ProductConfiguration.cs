using Amazon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Data.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK_Producto");

            builder.ToTable("Products");

            builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                .HasMaxLength(200);
            builder.Property(e => e.Description)
                .HasMaxLength(1000);
            builder.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
            builder.Property(e => e.Category)
                .HasMaxLength(100);
            builder.Property(e => e.ImageUrl)
                .HasMaxLength(500);
            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetime");

            builder.HasOne(d => d.Seller).WithMany(p => p.Products)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_User");
        }
    }
}
