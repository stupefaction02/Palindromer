using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Palindromer.Server.Services;

/// <summary>
/// Limits the number of incoming simultaneous requests to a specified quota.
/// </summary>
public class RequestLimiterMiddleware
{
    private RequestDelegate next;
    private readonly AppStatistics statistics;
    private readonly int maxRequestsCount;
    private ILogger<RequestLimiterMiddleware> logger;

    private object statisticsLocker = new object();

    public RequestLimiterMiddleware(RequestDelegate next, 
        int requestQuota,
        [FromServices] AppStatistics appStatistics,
        [FromServices] ILogger<RequestLimiterMiddleware> logger)
    {
        this.next = next;
        this.statistics = appStatistics;
        this.logger = logger;
        this.maxRequestsCount = requestQuota;
    }

    public Task Invoke(HttpContext context)
    {
        bool allowed = false;
        lock(statisticsLocker)
        {
            long requestsCount = statistics.RequestsCount;

            if ((requestsCount + 1) <= maxRequestsCount)
            {
                logger.LogInformation($"Request allowed. Current request count: {requestsCount}. Request quota: {maxRequestsCount}");
                statistics.RequestsCount++;
                allowed = true;
            }
            else
            {
                logger.LogInformation($"Request refused. Current request count: {requestsCount}. Request quota: {maxRequestsCount}");
                allowed = false;
            }
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