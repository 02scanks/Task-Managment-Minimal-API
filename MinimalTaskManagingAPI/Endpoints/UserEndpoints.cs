using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            // CREATE 
            app.MapPost("/api/user/create", async (AppDbContext _context, IUserService _userService, [FromBody] userDTO createUserDTO) => 
            {
                // Validate the model


                // Create new user object
                User newUser = new User()
                {
                    Username = createUserDTO.Username,
                    PasswordHash = _userService.HashPassword(createUserDTO.Password)
                };

                // add to db and save
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                return Results.Created($"/api/users/{newUser.Id}", newUser);
            });


            // LOGIN
            app.MapPost("/api/users/login", async (AppDbContext _context, IUserService _userService, [FromBody] userDTO userLoginDTO) => 
            {
                // validate model


                // check if user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userLoginDTO.Username);
                if (user == null)
                    return Results.NotFound("No user found with that username");

                // check if the supplied password matches the hashed password
                if (!_userService.VerifyPassword(user.PasswordHash, userLoginDTO.Password))
                    return Results.BadRequest("Password Incorrect");


                // if we made it this far then grant JWT Token 

                return Results.Accepted();
            });


        }
    }
}
