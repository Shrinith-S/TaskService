using MediatR;
using Microsoft.Extensions.Logging;

namespace TaskService.Application.Tasks.EventHandlers
{
    public class TaskCreatedEventHandler : INotificationHandler<TaskCreatedEvent>
    {
        private readonly ILogger<TaskCreatedEventHandler> _logger;

        public TaskCreatedEventHandler(ILogger<TaskCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Task Created: {Title}", notification.Title);
            return Task.CompletedTask;
        }
    }
}
