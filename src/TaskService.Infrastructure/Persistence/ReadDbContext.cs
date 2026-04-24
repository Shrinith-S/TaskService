using Microsoft.EntityFrameworkCore;

namespace TaskService.Infrastructure.Persistence
{
    public class ReadDbContext : DbContext
    {
        public ReadDbContext(DbContextOptions<ReadDbContext> options)
            : base(options) { }

        public DbSet<TaskReadModel> TaskReadModels => Set<TaskReadModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskReadModel>().ToTable("TaskReadModels");
        }
    }
}
