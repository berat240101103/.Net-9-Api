using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services;

public class UserService : IUserService{
    private readonly AppDbContext _context;
    public UserService(AppDbContext context){
        _context = context;}
    public async Task<List<UserResponseDto>> GetAllAsync(){
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new UserResponseDto{
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role ?? "User"
                }).ToListAsync();}
    public async Task<UserResponseDto?> GetByIdAsync(int id){
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null)
            return null;
        return new UserResponseDto{
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role ?? "User"};}
    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto){
        if (string.IsNullOrWhiteSpace(dto.Username))
            throw new ValidationException("Username is required");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException("Email is required");
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            throw new ValidationException("Password must be at least 8 characters long");
        var exists = await _context.Users.AnyAsync(u =>(u.Username == dto.Username || u.Email == dto.Email) && !u.IsDeleted);
        if (exists)
            throw new ValidationException("Username or email already exists");
        var user = new User{
            Username = dto.Username,
            Email = dto.Email,
            Password = dto.Password,
            Role = "User",
            CreatedAt = DateTime.UtcNow};
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return new UserResponseDto{
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role ?? "User"};}
    public async Task<bool> UpdateAsync(int id, UserUpdateDto dto){
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null)
            throw new KeyNotFoundException("User not found");
        if (string.IsNullOrWhiteSpace(dto.Username))
            throw new ValidationException("Username is required");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException("Email is required");
        user.Username = dto.Username;
        user.Email = dto.Email;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;}
    public async Task<bool> DeleteAsync(int id){
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null)
            throw new KeyNotFoundException("User not found");
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;}
    public async Task<bool> UpdateRoleAsync(int id, string role){
        if (role != "Admin" && role != "User")
            throw new ValidationException("Invalid role");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null)
            throw new KeyNotFoundException("User not found");
        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;}}
