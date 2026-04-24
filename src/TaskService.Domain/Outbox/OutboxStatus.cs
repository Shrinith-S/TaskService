namespace TaskService.Domain.Outbox;

public enum OutboxStatus
{
    Pending = 0,
    Processed = 1,
    Failed = 2
}