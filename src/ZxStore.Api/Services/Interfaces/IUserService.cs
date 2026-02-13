using ZxStore.Api.DTOs.User;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Services.Interfaces;

public interface IUserService
{
  Task<IEnumerable<User>> GetAllUserAsync();
  Task<IEnumerable<User>> GetUserByRoleAsync(int roleId);
  Task<User?> GetUserByIdAsync(Guid id);
  Task<User> CreateUserAsync(CreateUserDto request);
  Task<User?> UpdateUserAsync(Guid id, UpdateUserDto request);
  Task<bool> DeleteUserAsync(Guid id);
  Task<bool> UserExsistsAsync(Guid id);
}
