using MediatR;
using System.Text.Json;
using TaskService.Application.Common.Interfaces;
using TaskService.Contracts.Events;
using TaskService.Domain.Outbox;

namespace TaskService.Application.Tasks.Commands;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IRequestContext _requestContext;

    public CreateTaskCommandHandler(IApplicationDbContext dbContext, IRequestContext requestContext)
    {
        _dbContext = dbContext;
        _requestContext = requestContext;
    }

    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskItem(request.Title);
        var correlationId = _requestContext.CorrelationId;
        // 👇 Save Task using SAME DbContext
        await _dbContext.Tasks.AddAsync(task, cancellationToken);

        // 👇 Create Outbox Event
        var integrationEvent = new TaskCreatedIntegrationEvent(
                                    task.Id,
                                    task.Title,
                                    DateTime.UtcNow
                                );

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOn = DateTime.UtcNow,
            Type = typeof(TaskCreatedIntegrationEvent).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(integrationEvent),
            CorrelationId = correlationId,
            Status = OutboxStatus.Pending,
            RetryCount = 0
        };

        await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

        // 👇 SINGLE transaction
        await _dbContext.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}