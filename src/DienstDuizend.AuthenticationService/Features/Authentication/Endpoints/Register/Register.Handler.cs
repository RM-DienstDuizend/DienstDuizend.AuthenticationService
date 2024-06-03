using DienstDuizend.AuthenticationService.Features.Authentication.Domain;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain.ValueObjects;
using DienstDuizend.AuthenticationService.Infrastructure.Exceptions;
using DienstDuizend.AuthenticationService.Infrastructure.Persistence;
using DienstDuizend.Events;
using Isopoh.Cryptography.Argon2;
using MassTransit;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.Register;

[Handler]
public static partial class Register
{
    public record Command(
        Email Email,
        string FirstName,
        string LastName,
        string Password,
        string ConfirmPassword,
        bool AcceptedTermsOfService = false
    );
    
    public record Response(string Message = "Successfully registered!");
    
    private static async ValueTask<Response> HandleAsync(
        Command request,
        ApplicationDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        CancellationToken token)
    {
        if (request.AcceptedTermsOfService is false)
            throw Error.Failure("Registration.HasNotAcceptedTermsOfService",
                "Please read the terms of service, if you agree, please click accept.");
        
        // We handle it the same way as a successful registration, so hackers cannot detect if an email was already used before.
        if (await dbContext.Users.AnyAsync(u => u.Email == request.Email, token))
            return new Response();
        
        var user = new User()
        {
            Email = request.Email,
            HashedPassword = Argon2.Hash(request.Password),
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(token);
        
        // Mass Transit Event Here
        await publishEndpoint.Publish<UserRegisteredEvent>(new ()
        {
            UserId = user.Id,
            Email = user.Email.Value,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.Now
        }, token);
        
        return new Response();
    }
}

