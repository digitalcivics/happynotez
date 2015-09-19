using Microsoft.Data.Entity;

namespace HappyNotez.Models
{
    public class NotezContext : DbContext
    {
        public DbSet<Notez> Notez { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notez>().Property(b => b.Timestamp).Required();
            modelBuilder.Entity<Notez>().Property(b => b.Found).Required();
        }
    }
}
