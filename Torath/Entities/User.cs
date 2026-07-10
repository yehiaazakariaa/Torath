namespace Torath.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        // This will be used as their login username
        public string Email { get; set; } = string.Empty;

        // NEVER store passwords as plain text. This holds the scrambled (hashed) version.
        public string PasswordHash { get; set; } = string.Empty;

        // Determines permissions (e.g., "User" or "Admin"). Default is a standard user.
        public string Role { get; set; } = "User";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}