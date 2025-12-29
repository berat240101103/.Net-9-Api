using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistence{
    public class AppDbContext : DbContext{
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Review> Reviews => Set<Review>();
        protected override void OnModelCreating(ModelBuilder modelBuilder){
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("User");
            var seedDate = new DateTime(2026, 1, 1);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", CreatedAt = seedDate, UpdatedAt = DateTime.UtcNow },
                new Category { Id = 2, Name = "Books", CreatedAt = seedDate, UpdatedAt = DateTime.UtcNow });
            modelBuilder.Entity<User>().HasData(
                new User{Id = 1, Username = "admin", Email = "admin@test.com", Password = "123456", Role = "Admin", CreatedAt = seedDate});}}}
