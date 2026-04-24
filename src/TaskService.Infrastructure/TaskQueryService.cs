using Microsoft.EntityFrameworkCore;
using TaskService.Application.Common.Interfaces;
using TaskService.Application.Tasks.Queries;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Infrastructure;

public class TaskQueryService : ITaskQueryService
{
    private readonly RedisCacheService _cache;
    private readonly ReadDbContext _context;

    public TaskQueryService(ReadDbContext context, RedisCacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<TaskDto>> GetTasksAsync()
    {
        return await _context.TaskReadModels
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                IsCompleted = false
            })
            .ToListAsync();
    }

    public async Task<TaskDto?> GetTaskById(Guid id)
    {
        var cacheKey = $"task:{id}";

        var cached = await _cache.GetAsync<TaskDto>(cacheKey);
        if (cached != null)
        {
            Console.WriteLine("⚡ Cache HIT");
            return cached;
        }

        var task = await _context.TaskReadModels.FindAsync(id);

        if (task == null) return null;

        var dto = new TaskDto
        {
            Id = task.Id,
            Title = task.Title
        };

        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

        Console.WriteLine("🐢 Cache MISS → Read DB hit");

        return dto;
    }
}
