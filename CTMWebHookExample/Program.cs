using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/webhook", async context => {
  // Assuming 'authToken' is your known account secret key.
  var authToken = Environment.GetEnvironmentVariable("CTM_SECRET");
  /*
    remember to set the environment variable in your terminal with bash:
    export CTM_SECRET=your_actual_auth_token_here
    windows command prompt:
    set CTM_SECRET=your_actual_auth_token_here
    and windows powershell:
    $env:CTM_SECRET="your_actual_auth_token_here"
  */

      // Read the request time and signature from headers.
  var requestTime = context.Request.Headers["X-CTM-Time"];
  var receivedSignature = context.Request.Headers["X-CTM-Signature"];

  // Enable buffering for the request body to allow multiple reads.
  context.Request.EnableBuffering();

  // Read the request body.
  string requestBody;
  using (var reader = new StreamReader(context.Request.Body, leaveOpen: true)) {
    requestBody = await reader.ReadToEndAsync();
    // Reset the stream so it can be read again if needed.
    context.Request.Body.Position = 0;
  }

  // Compute the HMAC signature.
  var computedSignature = ComputeSignature(authToken, requestTime, requestBody);

  // Check if the computed signature matches the received signature.
  if (computedSignature == receivedSignature) {
    // now that we know the signature is valid, we can parse the request body
    try {
      var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
      };
      CTMWebHookPayload payload = JsonSerializer.Deserialize<CTMWebHookPayload>(requestBody, options);
      // Signature matches, proceed with your logic.
      await context.Response.WriteAsync("Signature verified. With payload: " + payload.Id);
    } catch (JsonException ex) { // Handle JSON parsing errors.
      context.Response.StatusCode = StatusCodes.Status400BadRequest;
      // write out th JsonException message to the response
      await context.Response.WriteAsync($"Signature verified - JSON parsing error: {ex.Message}");
      return;
    }
  } else {
    // Signature does not match, reject the request.
    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    await context.Response.WriteAsync("Signature verification failed.");
  }
});

app.Run();


static string ComputeSignature(string key, string requestTime, string requestBody) {
  using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key))) {
    var dataToSign = requestTime + requestBody;
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
    return Convert.ToBase64String(hash).Trim();
  }
}

public class CTMWebHookPayload {
  [JsonPropertyName("id")]
  public long Id { get; set; }

  [JsonPropertyName("name")]
  public string? Name { get; set; }

  [JsonPropertyName("caller_number")]
  public string? PhoneNumber { get; set; }
}
