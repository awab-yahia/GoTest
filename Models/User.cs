namespace GoTest.Models;

 
/// Represents a user in the system
 
public class User
{
     
    /// Primary key - unique identifier for each user
     
    public int Id { get; set; }

     
    /// User's email address
     
    public string Email { get; set; } = string.Empty;

     
    /// User's phone number
     
    public string PhoneNumber { get; set; } = string.Empty;

     
    /// Foreign key - links to the Role table
    /// This tells the database which role this user belongs to
     
    public int RoleId { get; set; }

     
    /// Navigation property - allows you to access the full Role object
    /// Example: user.Role.Name will give you the role name
     
    public Role? Role { get; set; }
}
