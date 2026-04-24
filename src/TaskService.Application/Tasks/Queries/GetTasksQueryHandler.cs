using MediatR;
using TaskService.Application.Common.Interfaces;

namespace TaskService.Application.Tasks.Queries;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, List<TaskDto>>
{
    private readonly ITaskQueryService _queryService;

    public GetTasksQueryHandler(ITaskQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<List<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetTasksAsync();
    }
}