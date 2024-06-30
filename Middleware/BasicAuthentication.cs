using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


public class BasicAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _username;
    private readonly string _password;

    public BasicAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _username = configuration["BasicAuth:Username"] ?? throw new ArgumentNullException(nameof(configuration));
        _password = configuration["BasicAuth:Password"] ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authorization header missing");
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authorization header missing");
            return;
        }

        var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader);
        var credentialBytes = Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
        var username = credentials[0];
        var password = credentials[1];

        if (username == _username && password == _password)
        {
            await _next(context);
        }
        else
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid credentials");
        }
    }
}

