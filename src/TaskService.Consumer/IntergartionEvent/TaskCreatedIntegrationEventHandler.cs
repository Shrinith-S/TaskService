using Prometheus;
using Rebus.Handlers;
using Rebus.Pipeline;
using TaskService.Consumer.Inbox;
using TaskService.Consumer.Persistence;
using TaskService.Contracts.Events;
using TaskService.Infrastructure;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Consumer.IntergartionEvent
{
    public class TaskCreatedIntegrationEventHandler : IHandleMessages<TaskCreatedIntegrationEvent>
    {
        private static readonly Counter ProcessedTasks = Metrics.CreateCounter("tasks_processed_total", "Total processed tasks");

        private readonly ConsumerDbContext _dbContext;
        private readonly ReadDbContext _readDb;
        private readonly IMessageContext _messageContext;
        private readonly ILogger<TaskCreatedIntegrationEventHandler> _logger;

        public TaskCreatedIntegrationEventHandler(
            ConsumerDbContext dbContext,
            ReadDbContext readDb,
            IMessageContext messageContext,
            ILogger<TaskCreatedIntegrationEventHandler> logger)
        {
            _dbContext = dbContext;
            _readDb = readDb;
            _messageContext = messageContext;
            _logger = logger;
        }

        public async Task Handle(TaskCreatedIntegrationEvent message)
        {
            var messageId = Guid.Parse(_messageContext.Headers["rbs2-msg-id"]);
            var correlationId = _messageContext.Headers["correlation-id"];
            // ✅ Check Inbox
            var exists = await _dbContext.InboxMessages.FindAsync(messageId);

            if (exists != null)
            {
                _logger.LogWarning("⚠️ Duplicate message ignored");
                return;
            }

            // ✅ Business Logic
            _logger.LogInformation("🔗 CorrelationId: {CorrelationId}", correlationId);
            _logger.LogInformation("🔥 Processing Task: {TaskId} - {Title}", message.TaskId, message.Title);

            var existing = await _readDb.TaskReadModels.FindAsync(message.TaskId);
            if (existing == null)
            {
                _readDb.TaskReadModels.Add(new TaskReadModel
                {
                    Id = message.TaskId,
                    Title = message.Title,
                    CreatedAt = message.CreatedAt
                });

                await _readDb.SaveChangesAsync();
            }


            // ✅ Save to Inbox
            _dbContext.InboxMessages.Add(new InboxMessage
            {
                Id = messageId,
                ReceivedAt = DateTime.UtcNow
            });
            
            // ✅ Increment Prometheus Counter
            ProcessedTasks.Inc();

            await _dbContext.SaveChangesAsync();
        }
    }
}
