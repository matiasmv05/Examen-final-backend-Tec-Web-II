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
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK_Pago");

            builder.ToTable("Payments");

            builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

            builder.Property(e => e.Status)
                .HasMaxLength(20);
            builder.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18,2)");
            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetime");

            builder.HasOne(d => d.Order).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Order");
        }
    }
}
