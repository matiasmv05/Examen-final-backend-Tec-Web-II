using Amazon.Core.Entities;
using Amazon.Core.Enum;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Data.Configuration
{
    public class SecurityConfiguration : IEntityTypeConfiguration<Security>
    {
       public void Configure(EntityTypeBuilder<Security> builder)
{
    builder.ToTable("Security");

    builder.Property(e => e.Name)
        .HasMaxLength(100);
    builder.Property(e => e.Password)
        .HasMaxLength(200);
    builder.Property(e => e.Role)
        .HasMaxLength(15)
        .HasConversion(
            x => x.ToString(),
            x => (RoleType)Enum.Parse(typeof(RoleType), x)
        );
    builder.Property(e => e.UserId).IsRequired();

    builder.HasOne(e => e.User)
        .WithOne()
        .HasForeignKey<Security>(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Ignore(e => e.Email);
}
    }

    
}
