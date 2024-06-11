using System.Net;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain.ValueObjects;
using DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.Register;
using DienstDuizend.AuthService.IntegrationTesting.Setup;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DienstDuizend.AuthenticationService.IntegrationTests.Features.AuthenticationTests;

public class RegisterTests : IntegrationTest
{
    private readonly Register.Handler _handler;

    public RegisterTests(WebAppFactory webAppFactory) : base(webAppFactory)
    {
        _handler = Scope.ServiceProvider.GetService<Register.Handler>();
    }

    [Fact]
    public async Task Register_CreatesUser_WhenInputIsValidAndUserDoesNotExist()
    {
        // Arrange
        var newUser = new Register.Command(
            Email.From("johndoe@mail.net"),
            "John",
            "Doe",
            "Password123!",
            "Password123!",
            true
        );
        
        // Act
        await _handler.HandleAsync(newUser);

        // Assert
        Db.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task Register_ReturnsValidationError_WhenRequestContentIsInvalid()
    {
        // Arrange
        var newUser = new Register.Command(
            Email.From("johndoe@mail.net"),
            "",
            "",
            "",
            "",
            true
        );

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(newUser);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        Db.Users.Should().HaveCount(0);
    }
    
    [Fact]
    public async Task Register_ReturnsException_WhenTermsOfServiceAcceptedIsFalse()
    {
        // Arrange
        var newUser = new Register.Command(
            Email.From("johndoe@mail.net"),
            "John",
            "Doe",
            "Password123!",
            "Password123!",
            false
        );

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(newUser);

        // Assert
        Db.Users.Should().HaveCount(0);
        var error = await act.Should().ThrowAsync<ApplicationException>();
        error.And.ErrorCode.Should().BeEquivalentTo("Registration.HasNotAcceptedTermsOfService"); 
    }

    [Fact]
    public async Task Register_DoesNotCreateUser_WhenInputIsValidAndUserDoesExist()
    {
        // Arrange
        var newUser = new Register.Command(
            Email.From("johndoe@mail.net"),
            "John",
            "Doe",
            "Password123!",
            "Password123!",
            true
        );

        Db.Users.Add(new User()
        {
            Email = newUser.Email,
            HashedPassword = newUser.Password
        });

        await Db.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(newUser);

        // Assert
        Db.Users.Should().HaveCount(1);
        await act.Should().NotThrowAsync();
    }
}