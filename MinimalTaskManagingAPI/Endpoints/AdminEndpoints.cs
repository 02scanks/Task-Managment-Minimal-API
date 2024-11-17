using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MinimalTaskManagingAPI.Data;
using MinimalTaskManagingAPI.Interfaces;
using MinimalTaskManagingAPI.Models;
using MinimalTaskManagingAPI.Models.DTO;
using MinimalTaskManagingAPI.Models.Paginated;

namespace MinimalTaskManagingAPI.Endpoints
{
    public class AdminEndpoints
    {
        public static void MapEndpoints(WebApplication app) 
        {
            // create new admin account 
            app.MapPost("/api/admin/create", [Authorize(Policy = "AdminOnly")] async (
                AppDbContext _context,
                IUserService _userService,
                IValidator<UserDTO> _validation,
                ICacheKeyService _cacheKeyService,
                [FromBody] UserDTO createUserDTO) =>
            {
                // Validate the model
                var modelState = await _validation.ValidateAsync(createUserDTO);
                if (!modelState.IsValid)
                    return Results.BadRequest(modelState.Errors.Select(e => e.ErrorMessage));

                // Create new user object
                User newUser = new User()
                {
                    Username = createUserDTO.Username,
                    PasswordHash = _userService.HashPassword(createUserDTO.Password),
                    Role = "Admin"
                };

                // add to db and save
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                // invalidate the all users cache keys
                _cacheKeyService.InvalidateStoredUserKeys();

                return Results.Created($"/api/users/{newUser.Id}", newUser);
            }).WithTags("Admins");



            // get all accounts in a cache-able paginated list
            app.MapGet("/api/admin/all-accounts", [Authorize(Policy = "AdminOnly")] async (
                AppDbContext _context,
                IMemoryCache _cache,
                ICacheKeyService _cacheKeyService,
                IUserService _userService,
                int pageNumber = 1, 
                int pageSize = 10) => 
            {
                // pagination setup
                if (pageNumber < 1)
                    return Results.BadRequest("Page number must be greater than 0");

                if (pageNumber > pageSize)
                    return Results.BadRequest("Page number exceeds total pages");

                // cache setup
                string cacheKey = $"all-users-page={pageNumber}size={pageSize}";

                // if no cache exists create one
                if (!_cache.TryGetValue(cacheKey, out IResult? paginatedUserList)) 
                {
                    paginatedUserList = await _userService.PaginateUsers(pageNumber, pageSize, _context);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    };

                    _cache.Set(cacheKey, paginatedUserList, cacheEntryOptions);
                    
                    // add the key to the cache key service list
                    _cacheKeyService.AddKeyToStoredUserKeys(cacheKey);
                }

                return Results.Ok(paginatedUserList);
            }).WithTags("Admins");
        }
    }
}
