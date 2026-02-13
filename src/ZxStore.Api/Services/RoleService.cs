using Microsoft.EntityFrameworkCore;
using ZxStore.Api.Data;
using ZxStore.Api.Entities;
using ZxStore.Api.Services.Interfaces;

namespace ZxStore.Api.Services;

public class RoleService : IRoleService
{
  private readonly AppDbContext _context;
  private readonly ILogger<RoleService> _logger;

  public RoleService(AppDbContext context, ILogger<RoleService> logger)
  {
    _context = context;
    _logger = logger;
  }

  public async Task<Role> CreateRoleAsync(string name)
  {
    if (await _context.Roles.AnyAsync(r => r.Name == name))
      throw new InvalidOperationException("Role already exists");

    var role = new Role
    {
      Name = name
    };

    _context.Roles.Add(role);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Role {RoleName} created", name);
    return role;
  }

  public async Task<bool> DeleteRoleAsync(int id)
  {
    var role = await _context.Roles
      .Include(r => r.UserRoles)
      .FirstOrDefaultAsync(r => r.Id == id);

    if (role is null) return false;
    if (role.UserRoles.Any(ur => ur.RoleId == id))
      throw new InvalidOperationException("Roles cannot be deleted, users use this role");

    _context.Roles.Remove(role);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Role {RoleId} has been deleted", id);
    return true;
  }

  public async Task<IEnumerable<Role>> GetAllRolesAsync()
  {
    return await _context.Roles
      .AsNoTracking()
      .ToListAsync();
  }

  public async Task<Role?> GetRoleByIdAsync(int id)
  {
    return await _context.Roles
      .AsNoTracking()
      .FirstOrDefaultAsync(r => r.Id == id);
  }

  public async Task<bool> RoleExistsAsync(int id)
  {
    return await _context.Roles.AnyAsync(r => r.Id == id);
  }

  public async Task<IEnumerable<Role>> SearchRolesAsync(string roleTerm)
  {
    return await _context.Roles
      .AsNoTracking()
      .Where(r => r.Name.Contains(roleTerm))
      .ToListAsync();
  }

  public async Task<Role?> UpdateRoleAsync(int id, string name)
  {
    var role = await _context.Roles.FindAsync(id);

    if (role is null) return null;

    role.Name = name;

    await _context.SaveChangesAsync();
    _logger.LogInformation("Role {RoleName} updated", name);

    return role;
  }
}
