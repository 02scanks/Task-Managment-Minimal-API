namespace MinimalTaskManagingAPI.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(string username, string role);
    }
}
