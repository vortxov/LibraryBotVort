using LibraryBot.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LibraryBot.Domain
{
    public class AppDbContext : DbContext 
    {
        public DbSet<Book> Books { get; set; } 
        public DbSet<Author> Authors { get; set; }
        public DbSet<PathBook> PathBooks { get; set; }
        public DbSet<Genres> Genres { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: false);

            var configuration = builder.Build();

            optionsBuilder.UseSqlServer(configuration["Db:ConnectionString"]); 
        }
    }
}
