using Microsoft.EntityFrameworkCore;

namespace Fibonacci.Calculator.Models
{
    public class FibonacciDbContext : DbContext
    {
        public DbSet<FibonacciResult> FibonacciResults { get; set; }

        public FibonacciDbContext(DbContextOptions<FibonacciDbContext> options)
        : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FibonacciResult>().HasKey(f => f.Number);
        }
    }
}
