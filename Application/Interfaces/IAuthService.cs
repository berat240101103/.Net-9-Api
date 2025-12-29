using Application.DTOs.Logins;

namespace Application.Interfaces;

public interface IAuthService {Task<string?> AuthenticateAsync(LoginDto login);}