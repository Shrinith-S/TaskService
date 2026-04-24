using TaskService.Domain.Common;

public class TaskCreatedEvent : BaseDomainEvent
{
    public Guid TaskId { get; }
    public string Title { get; }

    public TaskCreatedEvent(Guid taskId, string title)
    {
        TaskId = taskId;
        Title = title;
    }
}