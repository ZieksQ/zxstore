using ZxStore.Api.Entities;

namespace ZxStore.Api.Services.Interfaces;

public interface IUserService
{
  Task<IEnumerable<User>> GetAllUserAsync();
  Task<IEnumerable<User>> GetUserByRoleAsync(int roleId);
  Task<User?> GetUserByIdAsync(Guid id);
  Task<User> CreateUserAsync();
  Task<User?> UpdateUserAsync();
  Task<bool> DeleteUserAsync(Guid id);
  Task<bool> UserExsistsAsync(Guid id);
}
