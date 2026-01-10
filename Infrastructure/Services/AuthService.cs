using Application.DTOs.Logins;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class AuthService : IAuthService {
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    public AuthService(AppDbContext context, IConfiguration config) {
        _context = context;
        _config = config;}
    public async Task<string?> AuthenticateAsync(LoginDto login){
    var user = await _context.Users.FirstOrDefaultAsync(u =>u.Username == login.Username && !u.IsDeleted);
    if (user == null)
        return null;
    if (login.Password != user.Password)
        return null;
    var jwtKey = _config["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key is missing");
    var claims = new List<Claim>{
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim(ClaimTypes.Role, user.Role ?? "")};
    var tokenDescriptor = new SecurityTokenDescriptor{
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:DurationInMinutes"]!)),
        Issuer = _config["Jwt:Issuer"],
        Audience = _config["Jwt:Audience"],
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),SecurityAlgorithms.HmacSha256)};
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);}}

