using Microsoft.AspNetCore.Identity;
using MinimalTaskManagingAPI.Interfaces;

namespace MinimalTaskManagingAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IPasswordHasher<string> _passwordHasher;

        public UserService(IPasswordHasher<string> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, providedPassword);

            return result == PasswordVerificationResult.Success;
        }
    }
}
