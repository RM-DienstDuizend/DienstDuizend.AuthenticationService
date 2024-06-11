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

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.Login;

[Handler]
public static partial class Login
{
    public record Command(
        Email Email,
        string Password,
        string? OneTimePassword = null
    );

    public record Response(string AccessToken);

    private static async ValueTask<Response> HandleAsync(
        Command request,
        ApplicationDbContext dbContext,
        IJwtAuthTokenService jwtAuthTokenService,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        TwoFactorAuthenticator tfa,
        CancellationToken token)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, token);

        if (user is null) throw Error.Failure("User.InvalidCredentials", "The given email/password is invalid.");

        if (!Argon2.Verify(user.HashedPassword, request.Password))
        {
            user.FailedAttempts += 1;

            if (user.FailedAttempts == configuration.GetValue<int>("LockoutSettings:MaxAttempts"))
            {
                user.LockoutRemovalKey =
                    Nanoid.Generate(size: configuration.GetValue<int>("LockoutSettings:LockoutKeyLength"));

                await publishEndpoint.Publish(new UserTemporaryLockedOutEvent
                {
                    UserId = user.Id,
                    EmailAddress = user.Email.ToString(),
                    LockoutRemovalKey = user.LockoutRemovalKey
                });
            }

            await dbContext.SaveChangesAsync(token);

            throw Error.Failure("User.InvalidCredentials", "The given email/password is invalid.");
        }

        if (user.IsPermanentlyBlocked || user.LockoutRemovalKey is not null)
            throw Error.Forbidden("User.Blocked", "The given user has been (temporarily) blocked.");

        if (user.TwoFactorKey is not null)
        {
            if (request.OneTimePassword is null)
                throw Error.Failure("User.IncorrectCredentials", "Please provide a valid OTP code.");

            if (!tfa.ValidateTwoFactorPIN(user.TwoFactorKey, request.OneTimePassword))
                throw Error.Failure("User.IncorrectOTP", "The given OTP code is invalid");
        }

        user.FailedAttempts = 0;
        user.LastLogin = DateTime.Now.ToUniversalTime();
        await dbContext.SaveChangesAsync(token);


        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };


        // Refresh token
        httpContextAccessor.HttpContext?.Response.Cookies.Append(
            "DienstDuizend_RefreshToken",
            jwtAuthTokenService.GenerateRefreshToken(claims),
            new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow
                    .AddMinutes(
                        configuration.GetValue<int>("JwtAuthSettings:RefreshTokenExpirationTimeInMinutes")
                    )
            });

        return new Response(jwtAuthTokenService.GenerateAccessToken(claims));
    }
}