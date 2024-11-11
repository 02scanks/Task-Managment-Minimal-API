using Microsoft.EntityFrameworkCore;
using MinimalTaskManagingAPI.Models;

namespace MinimalTaskManagingAPI.Data
{
    public class AppDbContext : DbContext
    {

        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
