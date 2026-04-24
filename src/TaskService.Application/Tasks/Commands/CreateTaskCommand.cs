using MediatR;

namespace TaskService.Application.Tasks.Commands;

public record CreateTaskCommand(string Title) : IRequest<Guid>;