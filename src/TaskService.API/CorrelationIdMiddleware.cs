namespace TaskService.API
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Request.Headers["x-correlation-id"].FirstOrDefault()
                                 ?? Guid.NewGuid().ToString();

            context.Items["CorrelationId"] = correlationId;

            context.Response.Headers["x-correlation-id"] = correlationId;

            await _next(context);
        }
    }
}
