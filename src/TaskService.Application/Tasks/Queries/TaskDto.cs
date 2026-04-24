namespace TaskService.Application.Tasks.Queries;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}