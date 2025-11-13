namespace GoTest.Models;

 
/// Represents a role in the system (e.g., Admin, User, Manager)
 
public class Role
{
    /// Primary key - unique identifier for each role
     
    public int Id { get; set; }

    /// Name of the role (e.g., "Admin", "User")
     
    public string Name { get; set; } = string.Empty;

    /// Optional description of what this role does
     
    public string? Description { get; set; }

    /// Collection of users who have this role
    /// This is a "navigation property" - it creates a relationship between Role and User
     
    public ICollection<User> Users { get; set; } = new List<User>();
}
