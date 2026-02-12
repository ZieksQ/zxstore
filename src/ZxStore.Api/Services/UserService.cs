using Microsoft.EntityFrameworkCore;
using ZxStore.Api.Data;
using ZxStore.Api.Entities;
using ZxStore.Api.Services.Interfaces;

namespace ZxStore.Api.Services;

public class UserService : IUserService
{
  private readonly AppDbContext _context;
  private readonly ILogger<UserService> _logger;

  public UserService(
      AppDbContext context,
      ILogger<UserService> logger)
  {
    _context = context;
    _logger = logger;
  }

  public async Task<User> CreateUserAsync()
  {
    throw new NotImplementedException();
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
    throw new NotImplementedException();
  }

  public async Task<User?> UpdateUserAsync()
  {
    throw new NotImplementedException();
  }

  public async Task<bool> UserExsistsAsync(Guid id)
  {
    return await _context.Users.AnyAsync(u => u.Id == id);
  }
}
