namespace TaskService.Domain.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public DateTime OccurredOn { get; set; }

    public string Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public DateTime? ProcessedOn { get; set; }

    public OutboxStatus Status { get; set; }

    public int RetryCount { get; set; }

    public string? Error { get; set; }

    // Optional for multi-tenant systems
    public string? TenantId { get; set; }

    public string? CorrelationId { get; set; }
}