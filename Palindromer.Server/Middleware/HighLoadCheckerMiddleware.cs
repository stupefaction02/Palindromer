using Palindromer.Server.Services;

public class HighLoadCheckerMiddleware
{
    private RequestDelegate next;
    private readonly RequestsController highLoadController;
    private ILogger<HighLoadCheckerMiddleware> logger;

    public HighLoadCheckerMiddleware(RequestDelegate next, RequestsController highLoadController, ILogger<HighLoadCheckerMiddleware> logger)
    {
        this.next = next;
        this.highLoadController = highLoadController;
        this.logger = logger;
    }

    public Task Invoke(HttpContext context, RequestsController requestController)
    {
        bool allowed = false;
        lock (requestController.Locker)
        {
            if (requestController.CheckLimit())
            {
                requestController.RequestsCount++;
                logger.LogInformation($"Current requests count: {requestController.RequestsCount}");
            }

            logger.LogInformation($"Request refused. Current request count {requestController.RequestsCount} is greater than allowed {requestController.MaxRequestsCount}");
        }

        if (allowed)
        {
            return this.next(context);
        } 
        else
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return Task.CompletedTask;
        }
    }
}