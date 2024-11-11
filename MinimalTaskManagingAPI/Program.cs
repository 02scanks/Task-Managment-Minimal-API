using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinimalTaskManagingAPI.Data;
using MinimalTaskManagingAPI.Endpoints;
using MinimalTaskManagingAPI.Interfaces;
using MinimalTaskManagingAPI.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("taskdb");

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// custom services
builder.Services.AddDbContext<AppDbContext>(options => 
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// endpoints
UserEndpoints.MapEndpoints(app);

app.Run();