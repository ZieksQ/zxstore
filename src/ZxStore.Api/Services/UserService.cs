using Microsoft.EntityFrameworkCore;
using ZxStore.Api.Data;
using ZxStore.Api.DTOs.User;
using ZxStore.Api.Entities;
using ZxStore.Api.Services.Interfaces;

namespace ZxStore.Api.Services;

public class UserService : IUserService
{
  private readonly AppDbContext _context;
  private readonly ILogger<UserService> _logger;
  private readonly IRoleService _roleService;

  public UserService(
      AppDbContext context,
      ILogger<UserService> logger,
      IRoleService roleService)
  {
    _context = context;
    _logger = logger;
    _roleService = roleService;
  }

  // TODO: Create custom exception for conflict 409 
  // when user email already exists
  // NOTE: User Creation works like this:
  // 1. client request (username, password, email)
  // 2. check if user email exists, this is a safety net since we already have unique constraints
  //  - throw exception when email is already been used
  // 3. hash password using BCrypt.Net-Next
  // 4. create user object, then pass the properties
  // 5. add the new user in the database, then save changes
  // 6. the service returns the user
  public async Task<User> CreateUserAsync(CreateUserDto request)
  {
    if (await _context.Users.AnyAsync(u => u.Email == request.Email))
    {
      throw new InvalidOperationException("Email is already been used");
    }

    string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

    var user = new User
    {
      Username = request.Username,
      PasswordHash = passwordHash,
      Email = request.Email,
      CreatedAt = DateTime.UtcNow
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    _logger.LogInformation("User {Username} Created", request.Username);
    return user;
  }

  // WARNING: This type of delete is rather dangerous 
  // it may affect business logic, where every information
  // and transactions are important.
  public async Task<bool> DeleteUserAsync(Guid id)
  {
    var user = await _context.Users.FindAsync(id);

    if (user is null) return false;

    _context.Users.Remove(user);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Deleted User: {UserId}", id);
    return true;
  }

  public async Task<IEnumerable<User>> GetAllUserAsync()
  {
    return await _context.Users
      .AsNoTracking()
      .ToListAsync();
  }

  public async Task<User?> GetUserByIdAsync(Guid id)
  {
    return await _context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == id);
  }

  public async Task<IEnumerable<User>> GetUserByRoleAsync(int roleId)
  {
    if (await _roleService.RoleExistsAsync(roleId) is false)
      throw new InvalidOperationException("Role does not exists");

    return await _context.Users
      .AsNoTracking()
      .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
      .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
      .ToListAsync();
  }

  public async Task<User?> UpdateUserAsync(Guid id, UpdateUserDto request)
  {
    var user = await _context.Users
      .Include(u => u.UserRoles)
      .FirstOrDefaultAsync(u => u.Id == id);

    if (user is null) return null;

    if (request.Username != null)
    {
      user.Username = request.Username;
    }
    if (request.Password != null)
    {
      var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
      user.PasswordHash = passwordHash;
    }
    if (request.Email != null)
    {
      user.Email = request.Email;
    }

    await _context.SaveChangesAsync();
    return user;
  }

  public async Task<bool> UserExsistsAsync(Guid id)
  {
    return await _context.Users.AnyAsync(u => u.Id == id);
  }
}
