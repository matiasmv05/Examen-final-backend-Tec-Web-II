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
        public class UserConfiguration : IEntityTypeConfiguration<User>
        {
            public void Configure(EntityTypeBuilder<User> builder)
            {
                builder.HasKey(e => e.Id).HasName("PK_Usuario");

                builder.ToTable("Users");

            builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();


            builder.Property(e => e.Name)
                    .HasMaxLength(100);

                builder.Property(e => e.Email)
                    .HasMaxLength(255);

                builder.Property(e => e.Billetera)
                    .HasColumnType("decimal(18,2)");

                builder.Property(e => e.CreatedAt)
                    .HasColumnType("datetime");
        }
    }
}
