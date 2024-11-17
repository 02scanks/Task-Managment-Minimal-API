using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MinimalTaskManagingAPI.Data;
using MinimalTaskManagingAPI.Interfaces;
using MinimalTaskManagingAPI.Models;
using MinimalTaskManagingAPI.Models.DTO;

namespace MinimalTaskManagingAPI.Endpoints
{
    public class UserEndpoints
    {
        public static void MapEndpoints(WebApplication app) 
        {
            // Create
            app.MapPost("/api/user/create", async (
                AppDbContext _context, 
                IUserService _userService, 
                IValidator<UserDTO> _validation,
                ICacheKeyService _cacheKeyService,
                [FromBody] UserDTO createUserDTO) => 
            {
                // Validate the model
                var modelState = await _validation.ValidateAsync(createUserDTO);
                if(!modelState.IsValid)
                    return Results.BadRequest(modelState.Errors.Select(e => e.ErrorMessage));

                // Create new user object
                User newUser = new User()
                {
                    Username = createUserDTO.Username,
                    PasswordHash = _userService.HashPassword(createUserDTO.Password),
                    Role = "User"
                };

                // add to db and save
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                // invalidate all the stored user keys to prevent stale data
                _cacheKeyService.InvalidateStoredUserKeys();

                return Results.Created($"/api/users/{newUser.Id}", newUser);
            }).WithTags("Users");



            // Login
            app.MapPost("/api/user/login", async (
                AppDbContext _context, 
                IUserService _userService, 
                IJwtTokenService _tokenService,
                IValidator<UserDTO> _validation,
                [FromBody] UserDTO userLoginDTO) => 
            {
                // validate model
                var modelState = await _validation.ValidateAsync(userLoginDTO);
                if (!modelState.IsValid)
                    return Results.BadRequest(modelState.Errors.Select(e => e.ErrorMessage));

                // check if user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userLoginDTO.Username);
                if (user == null)
                    return Results.NotFound("No user found with that username");

                // check if the supplied password matches the hashed password
                if (!_userService.VerifyPassword(user.PasswordHash, userLoginDTO.Password))
                    return Results.BadRequest("Password Incorrect");

                // get user role
                string role = user.Role;
                if (string.IsNullOrEmpty(role))
                    return Results.NotFound("Invalid user role, please contact support");


                // if we made it this far then grant JWT Token 
                var token = _tokenService.GenerateToken(user.Username, role);

                return Results.Ok(new { token });
            }).WithTags("Users");



            // Read Account Details Of Current Authenticated User From JWT
            app.MapGet("/api/user/account-details", async (
                AppDbContext _context, 
                HttpContext httpContext, 
                IUserService _userService) => 
            {
                // get user model from Jwt Token
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);

                return Results.Ok(currentUser);

            }).WithTags("Users").RequireAuthorization();



            // Update account details Of Current Authenticated User From JWT
            app.MapPut("/api/user/update-account", async (
                AppDbContext _context, 
                HttpContext httpContext, 
                IUserService _userService,
                IValidator<UpdateUserDTO> _validation,
                ICacheKeyService _cacheKeyService,
                [FromBody] UpdateUserDTO updateUserDTO) => 
            {
                // validate the model
                var modelState = await _validation.ValidateAsync(updateUserDTO);
                if (!modelState.IsValid)
                    return Results.BadRequest(modelState.Errors.Select(e => e.ErrorMessage));

                // get model from JWT token
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);

                // validate the "oldPassword" field is correct before updating model
                if (!_userService.VerifyPassword(currentUser.PasswordHash, updateUserDTO.OldPassword))
                    return Results.BadRequest("Old password field is incorrect");

                // validate there's no other use with the new username
                if (await _context.Users.AnyAsync(u => u.Username.ToLower() == updateUserDTO.Username.ToLower() && u.Id != currentUser.Id))
                    return Results.BadRequest("Username already taken");

                // update the data
                currentUser.Username = updateUserDTO.Username;
                currentUser.PasswordHash = _userService.HashPassword(updateUserDTO.NewPassword);

                await _context.SaveChangesAsync();

                // invalidate all the all users cache keys to prevent staleness
                _cacheKeyService.InvalidateStoredUserKeys();

                return Results.Ok(updateUserDTO);
            }).WithTags("Users").RequireAuthorization();



            // Delete account
            app.MapDelete("/api/user/delete-account", async (
                AppDbContext _context, 
                HttpContext httpContext, 
                IUserService _userService, 
                ICacheKeyService _cacheKeyService) => 
            {
                // get current user from JWT Token
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);

                _context.Users.Remove(currentUser);
                await _context.SaveChangesAsync();

                // invalidate all the all users cache keys to prevent stale data
                _cacheKeyService.InvalidateStoredUserKeys();

                return Results.NoContent();

            }).WithTags("Users").RequireAuthorization();



           
        }
    }
}
