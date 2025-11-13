using Microsoft.EntityFrameworkCore;
using GoTest.Models;

namespace GoTest.Data;

 
/// Database Context - This is the main class that coordinates Entity Framework functionality
/// for your data model. It's like a session with the database.
 
public class ApplicationDbContext : DbContext
{
     
    /// Constructor - receives configuration options from dependency injection
     
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

     
    /// DbSet represents a table in the database
    /// You can use this to query and save Roles
    /// Example: _context.Roles.ToList() gets all roles
     
    public DbSet<Role> Roles { get; set; }

     
    /// DbSet for Users table
    /// Example: _context.Users.Where(u => u.Email == "test@test.com")
     
    public DbSet<User> Users { get; set; }

     
    /// This method is called when the model is being created
    /// Here you can configure relationships, constraints, etc.
     
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            // Make Email required and unique
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired();

            // Make PhoneNumber required
            entity.Property(u => u.PhoneNumber).IsRequired();

            // Configure the relationship between User and Role
            // One Role can have many Users
            entity.HasOne(u => u.Role)           // Each User has one Role
                  .WithMany(r => r.Users)        // Each Role has many Users
                  .HasForeignKey(u => u.RoleId)  // The foreign key is RoleId
                  .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a Role if it has Users
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            // Make Name required and unique
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Name).IsRequired();
        });
    }
}
