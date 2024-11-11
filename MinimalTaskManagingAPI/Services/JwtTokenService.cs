using MinimalTaskManagingAPI.Interfaces;

namespace MinimalTaskManagingAPI.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void GenerateToken(string username)
        {
            
        }
    }
}
