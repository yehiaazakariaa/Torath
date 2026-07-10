using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Torath.DTOs;
using Torath.Entities;

namespace Torath.Services
{
    public class AuthService : IAuthService
    {
        private readonly TorathDbContext _context;
        private readonly IConfiguration _configuration;

        // Inject the database and configuration (for the JWT key)
        public AuthService(TorathDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDto request)
        {
            // 1. Check if email is already taken
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return new AuthResponse { IsSuccess = false, Message = "User with this email already exists." };
            }

            // 2. Hash the password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Save the new user to the database
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponse { IsSuccess = true, Message = "User registered successfully!" };
        }

        public async Task<AuthResponse> LoginAsync(LoginDto request)
        {
            // 1. Look up the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return new AuthResponse { IsSuccess = false, Message = "Invalid email or password." };
            }

            // 2. Compare the typed password with the hashed password in the database
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new AuthResponse { IsSuccess = false, Message = "Invalid email or password." };
            }

            // 3. Generate the JWT Token if the password is correct
            string token = CreateToken(user);

            return new AuthResponse { IsSuccess = true, Message = "Login successful.", Token = token };
        }

        // --- Helper Method for JWT Generation ---
        private string CreateToken(User user)
        {
            // Claims are the data inside the token (like an ID card)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt:Issuer").Value,
                audience: _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Token lasts for 1 day
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}