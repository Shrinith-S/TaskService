using TaskService.Application.Tasks.Queries;

namespace TaskService.Application.Common.Interfaces
{
    public interface ITaskQueryService
    {
        Task<List<TaskDto>> GetTasksAsync();
        Task<TaskDto?> GetTaskById(Guid id);
    }
}
