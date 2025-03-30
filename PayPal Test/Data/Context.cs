using Microsoft.EntityFrameworkCore;

namespace PayPal_Test.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<PayPal_Test.Models.PayPalSettings> PayPalSettings { set; get; } = default!;
    }
}
