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
    public class Order_ItemConfiguration : IEntityTypeConfiguration<Order_Item>
    {
        public void Configure(EntityTypeBuilder<Order_Item> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK_OrdenItem");

            builder.ToTable("Order_Items");

            builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

            builder.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItem_Product");

            builder.HasOne(d => d.order)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade)  
            .HasConstraintName("FK_OrderItem_Order");
        }
    }
}
