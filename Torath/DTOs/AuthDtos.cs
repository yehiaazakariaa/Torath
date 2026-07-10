using System.ComponentModel.DataAnnotations;

namespace Torath.DTOs
{
    // What we expect when a user registers
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)] // Forces passwords to be at least 6 characters
        public string Password { get; set; } = string.Empty;
    }

    // What we expect when a user logs in
    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // The standardized response our Service will send back to the Controller
    public class AuthResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; // Holds the JWT if login succeeds
    }
}