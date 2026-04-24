namespace TaskService.Application.Common.Interfaces
{
    public interface IRequestContext
    {
        string? CorrelationId { get; }
    }
}
