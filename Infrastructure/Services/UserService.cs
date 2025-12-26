using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class UserService : IUserService{
    private readonly AppDbContext _context;
    public UserService(AppDbContext context){_context = context;}
    public async Task<List<UserResponseDto>> GetAllAsync(){
        return await _context.Users
            .Select(u => new UserResponseDto{
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            }).ToListAsync();}
    public async Task<UserResponseDto?> GetByIdAsync(int id){
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;
        return new UserResponseDto{
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };}
    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto){
        var user = new User{
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return new UserResponseDto{
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };}
    public async Task<bool> UpdateAsync(int id, UserUpdateDto dto){
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;
        user.Username = dto.Username;
        user.Email = dto.Email;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;}
    public async Task<bool> DeleteAsync(int id){
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;}}
