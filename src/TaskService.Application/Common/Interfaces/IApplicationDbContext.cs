using Microsoft.EntityFrameworkCore;
using TaskService.Domain.Outbox;

namespace TaskService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TaskItem> Tasks { get; }

    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}