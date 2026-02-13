using ZxStore.Api.Entities;

namespace ZxStore.Api.Services.Interfaces;

public interface IRoleService
{
  Task<IEnumerable<Role>> GetAllRolesAsync();
  Task<IEnumerable<Role>> SearchRolesAsync(string roleTerm);
  Task<Role?> GetRoleByIdAsync(int id);
  Task<Role> CreateRoleAsync(string name);
  Task<Role?> UpdateRoleAsync(int id, string name);
  Task<bool> DeleteRoleAsync(int id);
  Task<bool> RoleExistsAsync(int id);
}
