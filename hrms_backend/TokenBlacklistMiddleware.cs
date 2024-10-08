// TokenBlacklistMiddleware.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly BlacklistService _blacklistService;

    // Add a list of paths to exclude from token validation
    private readonly List<string> _excludePaths = new List<string>
    {
        "/api/auth/login",
        "/api/CompanyCreate/create",
        "/api/CompanyCreate/activate",
        "/api/logout" // Exclude logout endpoint
    };

    public TokenBlacklistMiddleware(RequestDelegate next, BlacklistService blacklistService)
    {
        _next = next;
        _blacklistService = blacklistService;
    }

    public async Task Invoke(HttpContext context)
    {
        // Get the request path
        var path = context.Request.Path;

        // Check if the path is in the exclude list
        if (_excludePaths.Contains(path))
        {
            await _next(context);
            return;
        }

        // Continue with token validation for other paths
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (_blacklistService.IsTokenBlacklisted(token))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Token is blacklisted");
            return;
        }

        await _next(context);
    }
}
