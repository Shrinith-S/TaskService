using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TaskService.Application.Common.Interfaces;

namespace TaskService.Application.Tasks.Queries
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
    {
        private readonly ITaskQueryService _queryService;

        public GetTaskByIdQueryHandler(ITaskQueryService queryService)
        {
            _queryService = queryService;
        }

        public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            return await _queryService.GetTaskById(request.Id);
        }
    }
}
