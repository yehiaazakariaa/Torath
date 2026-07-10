using Torath.DTOs;

namespace Torath.Services
{
    public interface IAuthService
    {
        // We promise the application that any class implementing this interface 
        // will have these two methods ready to use.
        Task<AuthResponse> RegisterAsync(RegisterDto request);
        Task<AuthResponse> LoginAsync(LoginDto request);
    }
}