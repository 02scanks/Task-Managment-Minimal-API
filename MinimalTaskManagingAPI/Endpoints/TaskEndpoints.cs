using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MinimalTaskManagingAPI.Data;
using MinimalTaskManagingAPI.Interfaces;
using MinimalTaskManagingAPI.Models;
using MinimalTaskManagingAPI.Models.DTO;
using MinimalTaskManagingAPI.Models.Paginated;

namespace MinimalTaskManagingAPI.Endpoints
{
    public class TaskEndpoints
    {
        public static void MapEndpoints(WebApplication app)
        {
            // Create task for current logged in user with JWT, Validation and cache invalidation
            app.MapPost("/api/user-tasks/create", async (
                AppDbContext _context, 
                HttpContext httpContext,
                IValidator<CreateTaskDTO> _validation,
                ICacheKeyService _cacheKeyService,
                IUserService _userService,
                [FromBody] CreateTaskDTO createTaskDTO) => 
            {
                // validate the model
                var modelState = await _validation.ValidateAsync(createTaskDTO);
                if (!modelState.IsValid)
                    return Results.BadRequest(modelState.Errors.Select(e => e.ErrorMessage));

                // get the current user from JWT 
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);
                if (currentUser == null)
                    return Results.Unauthorized();

                // create task object
                var task = new Models.Task() 
                {
                    UserId = currentUser.Id,
                    TaskName = createTaskDTO.TaskName,
                    Notes = createTaskDTO.Notes,
                    isComplete = false,
                    CompleteDate = createTaskDTO.CompleteDate,
                    CreationDate = DateTime.Now,
                    ModificationDate = DateTime.Now
                };

                // add task to user and save to db 
                currentUser.Tasks.Add(task);
                await _context.SaveChangesAsync();

                // clear all tasks cache key to prevent stale data



                return Results.Created($"/api/tasks/user-{currentUser.Id}/tasks/{task.Id}", createTaskDTO);

            }).WithTags("Tasks").RequireAuthorization();



            // Read specific task from current authenticated user with JWT with caching
            app.MapGet("/api/user-tasks/{taskId}", async(
                AppDbContext _context,
                HttpContext httpContext,
                IUserService _userService,
                IMemoryCache _cache,
                ICacheKeyService _cacheKeyService,
                int taskId) => 
            {
                // get current user model with tasks
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);
                if (currentUser == null)
                    return Results.Unauthorized();

                //define cache key
                string cacheKey = $"{currentUser.Username}-taskbyid={taskId}";

                if (!_cache.TryGetValue(cacheKey, out Models.Task? task)) 
                {
                    task = currentUser.Tasks.FirstOrDefault(t => t.Id == taskId);
                    if (task == null)
                        return Results.NotFound("No task was found");

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                    };

                    _cache.Set(cacheKey, task, cacheEntryOptions);

                    // add key to service list
                    _cacheKeyService.AddKeyToStoredTaskKeys(cacheKey);
                }

                return Results.Ok(task);

            }).WithTags("Tasks").RequireAuthorization();



            // Read all incomplete tasks from current user with cache validation and pagination
            app.MapGet("/api/user-tasks/incomplete", async (
                AppDbContext _context,
                HttpContext httpContext,
                IUserService _userService,
                ICacheKeyService _cacheKeyService,
                IMemoryCache _cache,
                int pageNumber = 1,
                int pageSize = 10) => 
            {
                if (pageNumber < 1)
                    return Results.BadRequest("Page size must be greater than 0");

                // get current user from token
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);
                if (currentUser == null)
                    return Results.Unauthorized();

                //define cache key
                string cacheKey = $"{currentUser.Username}-incomplete-tasks{pageNumber}&{pageSize}";

                // check if cache exists
                if (!_cache.TryGetValue(cacheKey, out IResult? paginatedTasksList)) 
                {
                    paginatedTasksList = _userService.PaginateTasks(pageNumber, pageSize, currentUser, false);

                    // define expiration time
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    };

                    _cache.Set(cacheKey, paginatedTasksList, cacheEntryOptions);
                }

                _cacheKeyService.AddKeyToStoredTaskKeys(cacheKey);

                return Results.Ok(paginatedTasksList);

            }).WithTags("Tasks").RequireAuthorization();



            // Read all complete tasks with cache validation and pagination
            app.MapGet("/api/user-tasks/complete", async (
                AppDbContext _context,
                HttpContext httpContext,
                IUserService _userService,
                ICacheKeyService _cacheKeyService,
                IMemoryCache _cache, 
                int pageNumber, 
                int pageSize) =>
            {
                if (pageNumber < 1)
                    return Results.BadRequest("Page number must be greater than 0");

                // get current user from token
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);
                if (currentUser == null)
                    return Results.Unauthorized();

                //define cache key
                string cacheKey = $"{currentUser.Username}-complete-tasks{pageNumber}&{pageSize}";

                // check if cache exists
                if (!_cache.TryGetValue(cacheKey, out IResult? pagedTasks))
                {
                    pagedTasks = _userService.PaginateTasks(pageNumber, pageSize, currentUser, true);

                    // define expiration time
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    };

                    _cache.Set(cacheKey, pagedTasks, cacheEntryOptions);
                }

                _cacheKeyService.AddKeyToStoredTaskKeys(cacheKey);

                return Results.Ok(pagedTasks);

            }).WithTags("Tasks").RequireAuthorization();



            // Read all tasks from current authenticated user with JWT with caching
            app.MapGet("/api/user-tasks/all", async (
                AppDbContext _context,
                HttpContext httpContext,
                IUserService _userService,
                IMemoryCache _cache,
                ICacheKeyService _cacheKeyService) =>
            {
                // get current user model with tasks
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);
                if (currentUser == null)
                    return Results.Unauthorized();

                //define cache key
                string cacheKey = $"{currentUser.Username}-alltasks";

                if (!_cache.TryGetValue(cacheKey, out IEnumerable<Models.Task>? tasks))
                {
                    tasks = currentUser.Tasks.ToList();
                    if (tasks == null)
                        return Results.NotFound("No tasks were found");

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    };

                    _cache.Set(cacheKey, tasks, cacheEntryOptions);

                    // add key to service list
                    _cacheKeyService.AddKeyToStoredTaskKeys(cacheKey);
                }

                return Results.Ok(tasks);

            }).WithTags("Tasks").RequireAuthorization();



            // Edit task details from current authenticated user with JWT with cache invalidation
            app.MapPut("/api/user-tasks/{taskId}/edit", async (
                AppDbContext _context, 
                HttpContext httpContext, 
                IValidator<UpdateTaskStatusDTO> _validation,
                IUserService _userService,
                ICacheKeyService _cacheKeyService,
                [FromBody] UpdateTaskStatusDTO updateTaskStatusDTO,
                int taskId) => 
            {
                var modelState = await _validation.ValidateAsync(updateTaskStatusDTO);
                if(!modelState.IsValid)
                    return Results.BadRequest(modelState.Errors.Select(e => e.ErrorMessage).ToList());

                // get current user from JWT
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);
                if (currentUser == null)
                    return Results.Unauthorized();

                // get task by id
                var taskToModify = currentUser.Tasks.FirstOrDefault(t => t.Id == taskId);
                if(taskToModify == null)
                    return Results.NotFound("No task found");

                // update the fields
                taskToModify.TaskName = taskToModify.TaskName;
                taskToModify.Notes = taskToModify.Notes;
                taskToModify.CompleteDate = taskToModify.CompleteDate;
                taskToModify.ModificationDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // clear task cache to avoid stale data in previous caches
                _cacheKeyService.InvalidStoredTaskKeys();

                return Results.Ok(taskToModify);
            }).WithTags("Tasks").RequireAuthorization();



            // Complete task by ID with current user JWT token and cache invalidation
            app.MapPut("/api/user-tasks/{taskId}/complete", async (
                AppDbContext _context,
                HttpContext httpContext,
                IUserService _userService,
                ICacheKeyService _cacheKeyService,
                int taskId) =>
            {
                // get current user from JWT
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);
                if (currentUser == null)
                    return Results.Unauthorized();

                // get task by id
                var taskToModify = currentUser.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (taskToModify == null)
                    return Results.NotFound("No task found");

                // complete the tasks
                taskToModify.isComplete = true;

                await _context.SaveChangesAsync();

                // clear task cache to avoid stale data in previous caches
                _cacheKeyService.InvalidStoredTaskKeys();

                return Results.Ok(taskToModify);
            }).WithTags("Tasks").RequireAuthorization();



            // Delete task from current authenticated user with JWT with cache invalidation
            app.MapDelete("/api/user-tasks/{taskId}/delete", async (
                AppDbContext _context, 
                HttpContext httpContext, 
                ICacheKeyService _cacheKeyService, 
                IUserService _userService,
                int taskId) =>
            {
                // get the current user
                User? currentUser = await _userService.GetUserFromTokenAsync(httpContext, _context);
                if (currentUser == null)
                    return Results.Unauthorized();

                // get the task by id
                var taskToDelete = currentUser.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (taskToDelete == null)
                    return Results.NotFound("No task found");

                // delete the task from user
                currentUser.Tasks.Remove(taskToDelete);
                await _context.SaveChangesAsync();

                return Results.NoContent();

            }).WithTags("Tasks").RequireAuthorization();
        }
    }
}
