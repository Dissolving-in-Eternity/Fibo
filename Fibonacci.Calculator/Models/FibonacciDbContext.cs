using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

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

    public class FibonacciDbContextFactory : IDesignTimeDbContextFactory<FibonacciDbContext>
    {
        public FibonacciDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<FibonacciDbContext>();
            var connectionString = configuration.GetConnectionString("Postgres");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Postgres' not found in configuration.");
            }

            optionsBuilder.UseNpgsql(connectionString);
            return new FibonacciDbContext(optionsBuilder.Options);
        }
    }
}
