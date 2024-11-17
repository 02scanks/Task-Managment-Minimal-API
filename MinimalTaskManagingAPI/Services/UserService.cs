using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinimalTaskManagingAPI.Data;
using MinimalTaskManagingAPI.Interfaces;
using MinimalTaskManagingAPI.Models;
using MinimalTaskManagingAPI.Models.Paginated;

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

        public async Task<User?> GetUserFromTokenAsync(HttpContext httpContext, AppDbContext _context) 
        {
            var usernameClaimId = httpContext.User
                .FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var currentUser = await _context.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Username == usernameClaimId);
                

            return currentUser ?? null;
        }

        public IResult PaginateTasks(int pageNumber, int pageSize, User currentUser, bool taskStatus) 
        {
            int skipAmount = (pageNumber - 1) * pageSize;

            var pagedTasks = currentUser.Tasks
            .Where(t => t.isComplete == taskStatus)
            .Skip(skipAmount)
            .Take(pageSize)
            .ToList();

            if (!pagedTasks.Any())
                return Results.NotFound("No tasks found");

            int totalTaskCount = pagedTasks.Count();
            int totalPages = (int)Math.Ceiling((double)totalTaskCount / pageSize);

            if (pageNumber > totalPages)
                return Results.BadRequest("Page number greater than total pages");

            var paginatedTasksList = new PaginatedTasksList()
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                PageCount = totalPages,
                TaskCount = totalTaskCount,
                Tasks = pagedTasks
            };

            return Results.Ok(paginatedTasksList);
        }

        public async Task<IResult> PaginateUsers(int pageNumber, int pageSize, AppDbContext _context)
        {
            int skipAmount = (pageNumber - 1) * pageSize;
            int totalUsersCount = await _context.Users.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalUsersCount / pageSize);

            var pagedUsers = await _context.Users
                        .Include(u => u.Tasks)
                        .Skip(skipAmount)
                        .Take(pageSize)
                        .ToListAsync();

            if (!pagedUsers.Any())
                return Results.BadRequest("No users found");

            var paginatedUserList = new PaginatedUserList()
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                PageCount = totalPages,
                UserCount = totalUsersCount,
                Users = pagedUsers
            };

            return Results.Ok(paginatedUserList);
        }
    }
}
