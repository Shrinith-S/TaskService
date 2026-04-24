using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskService.Application.Tasks.Queries
{
    public record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto?>;

}
