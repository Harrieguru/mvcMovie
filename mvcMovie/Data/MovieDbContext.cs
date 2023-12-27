using Microsoft.EntityFrameworkCore;
using mvcMovie.Models;

namespace mvcMovie.Data
{
    public class MovieDbContext : DbContext // <-- Make sure to inherit from DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; }  // This represents the Users table in the database

        // Additional DbSet properties for other models/entities go here


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Model customizations go here
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
