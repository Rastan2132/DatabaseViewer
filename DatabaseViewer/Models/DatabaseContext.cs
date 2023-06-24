using Microsoft.EntityFrameworkCore;

namespace DatabaseViewer.Models
{
    public class DatabaseContext : DbContext
    {
        private readonly string connectionString;

        public DbSet<Item> Items { get; set; }

        public DatabaseContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
