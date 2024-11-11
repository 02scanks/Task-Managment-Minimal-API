namespace MinimalTaskManagingAPI.Interfaces 
{
    public interface IUserService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
    }
}
