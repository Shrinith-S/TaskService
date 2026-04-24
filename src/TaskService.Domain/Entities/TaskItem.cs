using System.ComponentModel.DataAnnotations.Schema;
using TaskService.Domain.Common;

public class TaskItem
{
    private readonly List<BaseDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<BaseDomainEvent> DomainEvents => _domainEvents;

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public bool IsCompleted { get; private set; }

    private TaskItem() { }

    public TaskItem(string title)
    {
        Id = Guid.NewGuid();
        Title = title;
        IsCompleted = false;

        AddDomainEvent(new TaskCreatedEvent(Id, Title));
    }

    private void AddDomainEvent(BaseDomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}