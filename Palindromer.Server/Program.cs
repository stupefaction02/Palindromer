using Microsoft.AspNetCore.Mvc;
using Palindromer.Server.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        Console.Title = "Palindromer v1.0";
        Console.WriteLine("Booting up Palindromer v1.0 :)");

        IConfiguration configuration = builder.Configuration;

        ConfigureServices(builder, configuration);

        WebApplication app = builder.Build();

        app.UseMiddleware<HighLoadCheckerMiddleware>();

        app.MapGet("/palindromme", HandlePalindromeEndpoint);

        app.Run();
    }

    // approximately 100 mb
    private const int maxInput = 12500000;

    private static async Task<object> HandlePalindromeEndpoint(
            HttpContext context,
            [FromServices] RequestsController highLoadController,
            [FromServices] PalindromeService palindromeService)
    {
        if (context.Request.ContentLength > maxInput)
        {
            // Request Entity Too Large
            context.Response.StatusCode = 413;
            return "";
        }

        Stream content = context.Request.Body;

        string input;
        using (StreamReader streamReader = new StreamReader(content))
        {
            input = await streamReader.ReadToEndAsync();

            context.Response.StatusCode = 200;

            await Task.Delay(1000);
        }

        Interlocked.Decrement(ref highLoadController.RequestsCount);

        return new { isPalindrome = palindromeService.CheckPalindrome(input) };
    }

    private static void ConfigureServices(WebApplicationBuilder builder, IConfiguration configuration)
    {
        int maxRequests = int.Parse(configuration["MaxRequests"]);

        Console.WriteLine($"Max request count: {maxRequests}");

        RequestsController instance = new RequestsController(maxRequests);
        builder.Services.AddSingleton(instance);

        builder.Services.AddTransient<PalindromeService>();
    }
}