using MinimalTaskManagingAPI.Data;
using MinimalTaskManagingAPI.Models;

namespace MinimalTaskManagingAPI.Interfaces 
{
    public interface IUserService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
        Task<User?> GetUserFromTokenAsync(HttpContext httpContext, AppDbContext context);
        public IResult PaginateTasks(int pageNumber, int pageSize, User currentUser, bool taskStatus);
        public Task<IResult> PaginateUsers(int pageNumber, int pageSize, AppDbContext _context);
    }
}
