namespace MinimalTaskManagingAPI.Interfaces
{
    public interface IJwtTokenService
    {
        void GenerateToken(string username);
    }
}
