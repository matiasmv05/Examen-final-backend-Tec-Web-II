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
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK_Orden");

            builder.ToTable("Orders");

            builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

            builder.HasOne(d => d.User)
            .WithMany(p => p.Orders)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK_Order_User")
            .IsRequired(true);

            builder.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18,2)");
            builder.Property(e => e.Status)
                .HasMaxLength(20);            builder.Property(e => e.UpdatedAt)
                .HasColumnType("datetime");

            builder.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_User");
        }
    }
}
