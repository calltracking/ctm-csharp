using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var ctmSecret  = Environment.GetEnvironmentVariable("CTM_SECRET");
var ctmToken   = Environment.GetEnvironmentVariable("CTM_TOKEN");
var accountId  = Environment.GetEnvironmentVariable("CTM_ACCOUNT_ID");
var ctmHost    = Environment.GetEnvironmentVariable("CTM_HOST") ?? "app.calltrackingmetrics.com";

Console.WriteLine($"CTM_HOST: {ctmHost}");
Console.WriteLine($"CTM_TOKEN: {ctmToken}");
Console.WriteLine($"CTM_SECRET: {ctmSecret}");
Console.WriteLine($"CTM_ACCOUNT_ID: {accountId}");

// To access the phone our user will need to send an authenticated XHR request to this endpoint
app.MapPost("/ctm-phone-access", async context => {
    var requestUrl = $"https://{ctmHost}/api/v1/accounts/{accountId}/phone_access";

    // Dummy data for the request
    var email = "demo@example.com"; // This will be included in the response
    var requestData = new
    {
        email = email,
        first_name = "John",
        last_name = "Doe",
        session_id = "dummy_session_id" // This will be included in the response
    };

    var jsonContent = JsonSerializer.Serialize(requestData);
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

    using var httpClient = new HttpClient();
    var byteArray = Encoding.ASCII.GetBytes($"{ctmToken}:{ctmSecret}");
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

    var response = await httpClient.PostAsync(requestUrl, content);

    if (response.IsSuccessStatusCode)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;

        // Constructing new JSON object to include additional data
        var enhancedResponseData = new
        {
            status = root.TryGetProperty("status", out var status) ? status.GetString() : null,
            token = root.TryGetProperty("token", out var token) ? token.GetString() : null,
            valid_until = root.TryGetProperty("valid_until", out var validUntil) ? validUntil.GetInt32() : 0,
            sessionId = requestData.session_id, // Including the session_id from request
            email = requestData.email, // Including the email from the request
            last_name = requestData.last_name,
            first_name = requestData.first_name
        };

        var enhancedJsonResponse = JsonSerializer.Serialize(enhancedResponseData);

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(enhancedJsonResponse);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("Error accessing the phone access service");
    }
});

app.MapGet("/ctm-device", async context =>
{
    var filePath = Path.Combine(app.Environment.WebRootPath, "device.html");
    if (File.Exists(filePath))
    {
        var content = await File.ReadAllTextAsync(filePath);
        // Replace the placeholder with the actual environment variable
        content = content.Replace("%%CTM_HOST%%", ctmHost);

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(content);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("Page not found");
    }
});


app.MapGet("/", async context =>
{
    var filePath = Path.Combine(app.Environment.WebRootPath, "index.html");
    if (File.Exists(filePath))
    {
        var content = await File.ReadAllTextAsync(filePath);
        // Replace the placeholder with the actual environment variable
        content = content.Replace("%%CTM_HOST%%", ctmHost);

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(content);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("Page not found");
    }
});

app.MapGet("/favicon.ico", async context =>
{
    var faviconPath = Path.Combine(app.Environment.WebRootPath, "favicon.ico");
    if (File.Exists(faviconPath))
    {
        context.Response.ContentType = "image/x-icon";
        await context.Response.SendFileAsync(faviconPath);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("Favicon not found");
    }
});

app.MapGet("/app.js", async context =>
{
    var appPath = Path.Combine(app.Environment.WebRootPath, "app.js");
    if (File.Exists(appPath))
    {
        context.Response.ContentType = "application/javascript";
        await context.Response.SendFileAsync(appPath);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("Favicon not found");
    }
});

app.UseStaticFiles(); // Serve static files from wwwroot

app.Run();
