using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.Common.Interfaces;
using TaskService.Domain.Common;
using TaskService.Domain.Outbox;

namespace TaskService.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    private readonly IMediator _mediator;

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IMediator? mediator = null)  // 👈 make optional
        : base(options)
    {
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<BaseDomainEvent>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<TaskItem>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var events = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        return result;
    }
}