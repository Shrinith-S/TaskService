using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using System.Text.Json;
using TaskService.Domain.Outbox;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor started...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var bus = scope.ServiceProvider.GetRequiredService<IBus>();

                var messages = await dbContext.OutboxMessages
                    .Where(x => x.Status == OutboxStatus.Pending)
                    .OrderBy(x => x.OccurredOn)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                if (messages.Any())
                {
                    foreach (var message in messages)
                    {
                        try
                        {
                            // 👇 Deserialize event
                            var eventType = Type.GetType(message.Type);
                            var @event = JsonSerializer.Deserialize(message.Payload, eventType!);

                            // 👇 Publish to RabbitMQ
                            await bus.Send(@event, new Dictionary<string, string>
                                {
                                    { "correlation-id", message.CorrelationId ?? Guid.NewGuid().ToString() }
                                });

                            message.Status = OutboxStatus.Processed;
                            message.ProcessedOn = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message {Id}", message.Id);

                            message.RetryCount++;
                            message.Status = OutboxStatus.Failed;
                            message.Error = ex.Message;
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox Processor failed");
            }

            await Task.Delay(5000, stoppingToken); // poll every 5 sec
        }
    }
}