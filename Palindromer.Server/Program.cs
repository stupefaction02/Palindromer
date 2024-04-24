using Microsoft.AspNetCore.Mvc;
using Palindromer.Server.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        Console.Title = "Palindromer.Server v1.0";
        Console.WriteLine("Booting up Palindromer v1.0 :)");

        IConfiguration configuration = builder.Configuration;

        ConfigureServices(builder, configuration);

        WebApplication app = builder.Build();

        UseRequestLimiterMiddleware(app, configuration);

        app.MapGet("/palindromme", HandlePalindromeEndpoint);

        app.Run();
    }

    private static void UseRequestLimiterMiddleware(WebApplication app, IConfiguration configuration)
    {
        string q = configuration["RequestsQuota"];
        if (q != null)
        {
            int requestQuota = int.Parse(q);
            if (requestQuota < 0) requestQuota = 1;

            Console.WriteLine($"Request quota: {requestQuota}");

            app.UseMiddleware<RequestLimiterMiddleware>(requestQuota);
        }
    }

    // approximately 100 mb
    private const int maxInput = 12500000;

    private static async Task<object?> HandlePalindromeEndpoint(
            HttpContext context,
            [FromServices] AppStatistics statistics,
            [FromServices] PalindromeService palindromeService)
    {
        if (context.Request.ContentLength == 0)
        {
            // Bad Request
            context.Response.StatusCode = 400;
            return default;
        }
        else if (context.Request.ContentLength > maxInput)
        {
            // Request Entity Too Large
            context.Response.StatusCode = 413;
            return default;
        }

        Stream content = context.Request.Body;
        string input;
        using (StreamReader streamReader = new StreamReader(context.Request.Body))
        {
            input = await streamReader.ReadToEndAsync();

            // usefull work ;-)
            await Task.Delay(1000);
        }

        // deletes request from statistics after it has completed
        context.Response.RegisterForDispose(new PostRequestAction 
        { 
            Action = () => 
            {
                Interlocked.Decrement(ref statistics.RequestsCount);
            }
        });

        return new { isPalindrome = palindromeService.CheckPalindrome(input) };
    }

    private static void ConfigureServices(WebApplicationBuilder builder, IConfiguration configuration)
    {
        AppStatistics instance = new AppStatistics();
        builder.Services.AddSingleton(instance);

        builder.Services.AddTransient<PalindromeService>();
    }

    public class PostRequestAction : IDisposable
    {
        public Action Action { get; set; }
        public void Dispose()
        {
            Action?.Invoke();
        }
    }
}