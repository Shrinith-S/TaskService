using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Infrastructure;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=IN-PW03CRZB\\MSSQLSERVER0201;Database=TaskServiceDb;User Id=sa;Password=sa@123456789;TrustServerCertificate=True");

        return new AppDbContext(optionsBuilder.Options);
    }
}