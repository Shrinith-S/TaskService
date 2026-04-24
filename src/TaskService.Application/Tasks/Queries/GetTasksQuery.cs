using MediatR;

namespace TaskService.Application.Tasks.Queries;

public record GetTasksQuery() : IRequest<List<TaskDto>>;
