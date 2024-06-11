using System.Security.Claims;
using DienstDuizend.AuthenticationService.Common.Interfaces;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain.ValueObjects;
using DienstDuizend.AuthenticationService.Infrastructure.Exceptions;
using DienstDuizend.AuthenticationService.Infrastructure.Persistence;
using DienstDuizend.Events;
using Google.Authenticator;
using Isopoh.Cryptography.Argon2;
using MassTransit;
using NanoidDotNet;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.RefreshToken;

[Handler]
public static partial class RefreshToken
{
    public record Command;

    public record Response(string AccessToken);

    private static async ValueTask<Response> HandleAsync(
        Command request,
        IJwtAuthTokenService jwtAuthTokenService,
        IHttpContextAccessor httpContextAccessor,
        CancellationToken token)
    {

        if (!httpContextAccessor.HttpContext.Request.Cookies
                .TryGetValue("DienstDuizend_RefreshToken", out string refreshToken))
            throw Error.Failure("AccessToken.UnableToRefresh", "Unable to refresh access token.");


        var accessToken = jwtAuthTokenService.GenerateAccessTokenWithRefreshToken(refreshToken);

        return new Response(accessToken);
    }
}