using DienstDuizend.AuthenticationService.Features.Authentication.Domain;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain.ValueObjects;
using DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.Login;
using DienstDuizend.AuthService.IntegrationTesting.Setup;
using Google.Authenticator;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NanoidDotNet;
using Xunit;

namespace DienstDuizend.AuthenticationService.IntegrationTests.Features.AuthenticationTests;

public class LoginTests : IntegrationTest
{
    private readonly Login.Handler _handler;
    private readonly TwoFactorAuthenticator _twoFactorAuthenticator;

    public LoginTests(WebAppFactory webAppFactory) : base(webAppFactory)
    {
        _handler = Scope.ServiceProvider.GetRequiredService<Login.Handler>();
        _twoFactorAuthenticator = Scope.ServiceProvider.GetRequiredService<TwoFactorAuthenticator>();
    }

    [Fact]
    public async Task Login_ReturnsAccessToken_WhenCredentialsAreValidAndUserIsNotBlocked()
    {
        // Arrange
        var credentials = new Login.Command(
            Email.From("johndoe@mail.net"),
            "Password123!"
        );

        Db.Users.Add(new User()
        {
            Email = credentials.Email,
            HashedPassword = Argon2.Hash(credentials.Password),
        });

        await Db.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(credentials);

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ThrowsInvalidCredentialsException_WhenEmailIsInvalid()
    {
        // Arrange
        var credentials = new Login.Command(
            Email.From("johndoe@mail.net"),
            "Password123!"
        );

        // Act
        Func<Task<Login.Response>> act = async () => await _handler.HandleAsync(credentials);

        // Assert
        var error = await act.Should().ThrowAsync<ApplicationException>();
        error.And.ErrorCode.Should().Contain("User.InvalidCredentials");
    }

    [Fact]
    public async Task Login_ThrowsInvalidCredentialsException_WhenPasswordIsInvalid()
    {
        // Arrange
        var credentials = new Login.Command(
            Email.From("johndoe@mail.net"),
            "Password123!"
        );

        Db.Users.Add(new User()
        {
            Email = credentials.Email,
            HashedPassword = Argon2.Hash("OtherPassword567@"),
        });

        await Db.SaveChangesAsync();

        // Act
        Func<Task<Login.Response>> act = async () => await _handler.HandleAsync(credentials);

        // Assert
        var error = await act.Should().ThrowAsync<ApplicationException>();
        error.And.ErrorCode.Should().Contain("User.InvalidCredentials");
    }

    [Fact]
    public async Task Login_ThrowsUserBlockedException_WhenCredentialsAreValidButUserIsBlocked()
    {
        // Arrange
        var credentials = new Login.Command(
            Email.From("johndoe@mail.net"),
            "Password123!"
        );

        Db.Users.Add(new User()
        {
            Email = credentials.Email,
            HashedPassword = Argon2.Hash(credentials.Password),
            LockoutRemovalKey = await Nanoid.GenerateAsync(size: 3)
        });

        await Db.SaveChangesAsync();

        // Act
        Func<Task<Login.Response>> act = async () => await _handler.HandleAsync(credentials);

        // Assert
        var error = await act.Should().ThrowAsync<ApplicationException>();
        error.And.ErrorCode.Should().Contain("User.Blocked");
    }

    [Fact]
    public async Task Login_ThrowsException_WhenCredentialsAreValidButUserIsPermanentlyBlocked()
    {
        // Arrange
        var credentials = new Login.Command(
            Email.From("johndoe@mail.net"),
            "Password123!"
        );

        Db.Users.Add(new User()
        {
            Email = credentials.Email,
            HashedPassword = Argon2.Hash(credentials.Password),
            IsPermanentlyBlocked = true
        });

        await Db.SaveChangesAsync();

        // Act
        Func<Task<Login.Response>> act = async () => await _handler.HandleAsync(credentials);

        // Assert
        var error = await act.Should().ThrowAsync<ApplicationException>();
        error.And.ErrorCode.Should().Contain("User.Blocked");
    }

    [Fact]
    public async Task Login_ThrowsIncorrectCredentialsException_WhenOtpIsRequiredButNotProvided()
    {
        // Arrange
        var credentials = new Login.Command(
            Email.From("johndoe@mail.net"),
            "Password123!"
        );

        Db.Users.Add(new User()
        {
            Email = credentials.Email,
            HashedPassword = Argon2.Hash(credentials.Password),
            TwoFactorKey = "validkey"
        });

        await Db.SaveChangesAsync();

        // Act
        Func<Task<Login.Response>> act = async () => await _handler.HandleAsync(credentials);

        // Assert
        var error = await act.Should().ThrowAsync<ApplicationException>();
        error.And.ErrorCode.Should().Contain("User.IncorrectCredentials");
    }

    [Fact]
    public async Task Login_ThrowsIncorrectOtpException_WhenOtpIsInvalid()
    {
        // Arrange
        var secretKey = "SUPERSECRETPINKEY";
        
        var credentials = new Login.Command(
            Email.From("johndoe@mail.net"),
            "Password123!",
            "999999"
        );

        var user = new User()
        {
            Email = credentials.Email,
            HashedPassword = Argon2.Hash(credentials.Password),
            TwoFactorKey = secretKey
        };

        Db.Users.Add(user);
        await Db.SaveChangesAsync();
        


        // Act
        Func<Task<Login.Response>> act = async () => await _handler.HandleAsync(credentials);

        // Assert
        var error = await act.Should().ThrowAsync<ApplicationException>();
        error.And.ErrorCode.Should().Contain("User.IncorrectOTP");
    }

    [Fact]
    public async Task Login_ReturnsAccessToken_WhenOtpIsValid()
    {
        // Arrange
        var secretKey = "SUPERSECRETPINKEY";
        var currentPin = _twoFactorAuthenticator.GetCurrentPIN(secretKey);
        
        var credentials = new Login.Command(
            Email.From("johndoe@mail.net"),
            "Password123!",
            currentPin
        );

        var user = new User()
        {
            Email = credentials.Email,
            HashedPassword = Argon2.Hash(credentials.Password),
            TwoFactorKey = secretKey
        };

        Db.Users.Add(user);
        await Db.SaveChangesAsync();
        

        // Act
        var result = await _handler.HandleAsync(credentials);

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ThrowsUserBlockedException_WhenFailedAttemptsExceedMaxAttempts()
    {
        // Arrange
        var credentials = new Login.Command(
            Email.From("johndoe@mail.net"),
            "Password123!"
        );

        var userId = Guid.NewGuid();

        var user = new User()
        {
            Id = userId,
            Email = credentials.Email,
            HashedPassword = Argon2.Hash("Password123!"),
            FailedAttempts = 3,
            LockoutRemovalKey = "ABC"
        };

        Db.Users.Add(user);
        await Db.SaveChangesAsync();

        // Act
        Func<Task<Login.Response>> act = async () => await _handler.HandleAsync(credentials);

        // Assert
        var error = await act.Should().ThrowAsync<ApplicationException>();
        error.And.ErrorCode.Should().Contain("User.Blocked");

        var updatedUser = await Db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        updatedUser.Should().NotBeNull();
        updatedUser.FailedAttempts.Should().Be(4);
        updatedUser.LockoutRemovalKey.Should().NotBeNull();
    }
}
