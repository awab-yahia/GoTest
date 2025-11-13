using Microsoft.EntityFrameworkCore;
using GoTest.Data;
using GoTest.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the database context with PostgreSQL
// This tells the app to use PostgreSQL and where to find the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ==================== ROLES API ENDPOINTS ====================

// GET /api/roles - Get all roles
app.MapGet("/api/roles", async (ApplicationDbContext db) =>
{
    // Return all roles from the database
    return await db.Roles.ToListAsync();
})
.WithName("GetAllRoles")
.WithTags("Roles");

// GET /api/roles/{id} - Get a specific role by ID
app.MapGet("/api/roles/{id}", async (int id, ApplicationDbContext db) =>
{
    // Find the role by ID
    var role = await db.Roles.FindAsync(id);

    // If not found, return 404 Not Found
    if (role == null)
        return Results.NotFound(new { message = $"Role with ID {id} not found" });

    // Return the role
    return Results.Ok(role);
})
.WithName("GetRoleById")
.WithTags("Roles");

// POST /api/roles - Create a new role
app.MapPost("/api/roles", async (Role role, ApplicationDbContext db) =>
{
    // Add the role to the database
    db.Roles.Add(role);
    await db.SaveChangesAsync();

    // Return 201 Created with the new role
    return Results.Created($"/api/roles/{role.Id}", role);
})
.WithName("CreateRole")
.WithTags("Roles");

// PUT /api/roles/{id} - Update an existing role
app.MapPut("/api/roles/{id}", async (int id, Role updatedRole, ApplicationDbContext db) =>
{
    // Find the existing role
    var role = await db.Roles.FindAsync(id);

    if (role == null)
        return Results.NotFound(new { message = $"Role with ID {id} not found" });

    // Update the properties
    role.Name = updatedRole.Name;
    role.Description = updatedRole.Description;

    // Save changes
    await db.SaveChangesAsync();

    return Results.Ok(role);
})
.WithName("UpdateRole")
.WithTags("Roles");

// DELETE /api/roles/{id} - Delete a role
app.MapDelete("/api/roles/{id}", async (int id, ApplicationDbContext db) =>
{
    // Find the role
    var role = await db.Roles.FindAsync(id);

    if (role == null)
        return Results.NotFound(new { message = $"Role with ID {id} not found" });

    // Remove the role
    db.Roles.Remove(role);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = $"Role '{role.Name}' deleted successfully" });
})
.WithName("DeleteRole")
.WithTags("Roles");

// ==================== USERS API ENDPOINTS ====================

// GET /api/users - Get all users (includes their role information)
app.MapGet("/api/users", async (ApplicationDbContext db) =>
{
    // Include() loads the related Role data for each user
    return await db.Users.Include(u => u.Role).ToListAsync();
})
.WithName("GetAllUsers")
.WithTags("Users");

// GET /api/users/{id} - Get a specific user by ID
app.MapGet("/api/users/{id}", async (int id, ApplicationDbContext db) =>
{
    // Find the user and include their role
    var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

    if (user == null)
        return Results.NotFound(new { message = $"User with ID {id} not found" });

    return Results.Ok(user);
})
.WithName("GetUserById")
.WithTags("Users");

// POST /api/users - Create a new user
app.MapPost("/api/users", async (User user, ApplicationDbContext db) =>
{
    // Check if the role exists
    var roleExists = await db.Roles.AnyAsync(r => r.Id == user.RoleId);
    if (!roleExists)
        return Results.BadRequest(new { message = $"Role with ID {user.RoleId} does not exist" });

    // Check if email already exists
    var emailExists = await db.Users.AnyAsync(u => u.Email == user.Email);
    if (emailExists)
        return Results.BadRequest(new { message = $"User with email '{user.Email}' already exists" });

    // Add the user
    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Reload the user with role information
    var createdUser = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == user.Id);

    return Results.Created($"/api/users/{user.Id}", createdUser);
})
.WithName("CreateUser")
.WithTags("Users");

// PUT /api/users/{id} - Update an existing user
app.MapPut("/api/users/{id}", async (int id, User updatedUser, ApplicationDbContext db) =>
{
    // Find the existing user
    var user = await db.Users.FindAsync(id);

    if (user == null)
        return Results.NotFound(new { message = $"User with ID {id} not found" });

    // Check if the new role exists
    var roleExists = await db.Roles.AnyAsync(r => r.Id == updatedUser.RoleId);
    if (!roleExists)
        return Results.BadRequest(new { message = $"Role with ID {updatedUser.RoleId} does not exist" });

    // Check if email already exists (for a different user)
    var emailExists = await db.Users.AnyAsync(u => u.Email == updatedUser.Email && u.Id != id);
    if (emailExists)
        return Results.BadRequest(new { message = $"User with email '{updatedUser.Email}' already exists" });

    // Update the properties
    user.Email = updatedUser.Email;
    user.PhoneNumber = updatedUser.PhoneNumber;
    user.RoleId = updatedUser.RoleId;

    await db.SaveChangesAsync();

    // Reload with role information
    var result = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

    return Results.Ok(result);
})
.WithName("UpdateUser")
.WithTags("Users");

// DELETE /api/users/{id} - Delete a user
app.MapDelete("/api/users/{id}", async (int id, ApplicationDbContext db) =>
{
    // Find the user
    var user = await db.Users.FindAsync(id);

    if (user == null)
        return Results.NotFound(new { message = $"User with ID {id} not found" });

    // Remove the user
    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = $"User with email '{user.Email}' deleted successfully" });
})
.WithName("DeleteUser")
.WithTags("Users");

app.Run();
