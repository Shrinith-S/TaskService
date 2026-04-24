namespace TaskService.Application.Common.Interfaces;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task);
}