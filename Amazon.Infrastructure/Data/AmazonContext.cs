using Amazon.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Amazon.Infrastructure.Data
{
    
    /// /Base de datos de Amazon
    public partial class AmazonContext : DbContext
    {
        public AmazonContext()
        { }

        public AmazonContext(DbContextOptions<AmazonContext> options) : base(options) { }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Order_Item> Order_Items { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Security> Securities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }


    }
       
    }
