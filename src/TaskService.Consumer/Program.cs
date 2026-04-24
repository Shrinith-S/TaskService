using Microsoft.EntityFrameworkCore;
using Prometheus;
using Rebus.Config;
using Serilog;
using TaskService.Consumer.IntergartionEvent;
using TaskService.Consumer.Persistence;
using TaskService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddRebus(configure => configure
    .Transport(t => t.UseRabbitMq(
        builder.Configuration.GetConnectionString("RabbitMq"),
        "taskservice-consumer-queue"))
);
builder.Services.AddDbContext<ReadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<ConsumerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AutoRegisterHandlersFromAssemblyOf<TaskCreatedIntegrationEventHandler>();

var app = builder.Build();

app.UseHttpMetrics();     // tracks HTTP metrics
app.MapMetrics();         // exposes /metrics endpoint

app.Run();