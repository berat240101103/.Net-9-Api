using Application.DTOs.Users;

namespace Application.Interfaces;

public interface IUserService{
    Task<List<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<UserResponseDto> CreateAsync(UserCreateDto dto);
    Task<bool> UpdateAsync(int id, UserUpdateDto dto);
    Task<bool> DeleteAsync(int id);}