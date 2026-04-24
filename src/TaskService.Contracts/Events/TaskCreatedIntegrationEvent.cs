namespace TaskService.Contracts.Events;

public record TaskCreatedIntegrationEvent(
    Guid TaskId,
    string Title,
    DateTime CreatedAt
);