using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Serilog;
using StackExchange.Redis;
using TaskService.API;
using TaskService.Application;
using TaskService.Application.Common.Interfaces;
using TaskService.Application.Tasks.Commands;
using TaskService.Contracts.Events;
using TaskService.Infrastructure;
using TaskService.Infrastructure.Outbox;
using TaskService.Infrastructure.Persistence;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<ReadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddRebus(configure => configure
    .Transport(t => t.UseRabbitMq(
        builder.Configuration.GetConnectionString("RabbitMq"),
        "taskservice-producer"))
    .Routing(r => r.TypeBased()
        .Map<TaskCreatedIntegrationEvent>("taskservice-consumer-queue"))
);

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddScoped<IApplicationDbContext, AppDbContext>();
builder.Services.AddHostedService<OutboxProcessor>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateTaskCommand).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(CreateTaskCommandValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskQueryService, TaskQueryService>();
builder.Services.AddScoped<RedisCacheService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, RequestContext>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpMetrics();     // tracks HTTP metrics
app.MapMetrics();         // exposes /metrics endpoint

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.ContentType = "application/json";

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var errors = validationException.Errors.Select(e => new
            {
                field = e.PropertyName,
                message = e.ErrorMessage
            });

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                errors
            });

            return;
        }

        context.Response.StatusCode = 500;

        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "Internal Server Error"
        });
    });
});

app.UseMiddleware<CorrelationIdMiddleware>();

app.MapControllers();
app.Run();