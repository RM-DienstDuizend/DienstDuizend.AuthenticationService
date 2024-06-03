using DienstDuizend.AuthenticationService.Common.Interfaces;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain;
using DienstDuizend.AuthenticationService.Infrastructure.Exceptions;
using DienstDuizend.AuthenticationService.Infrastructure.Persistence;
using DienstDuizend.Events;
using Immediate.Handlers.Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.DeleteAccount;

[Handler]
public static partial class DeleteAccount
{
    public record Command(bool AreYouSure);

    public record Response(string Message = "Your account has been successfully removed. Goodbye :(");
    
    private static async ValueTask<Response> HandleAsync(
        Command request,
        ApplicationDbContext dbContext,
        ICurrentUserProvider currentUserProvider,
        IPublishEndpoint publishEndpoint,
        CancellationToken token)
    {

        if (!request.AreYouSure)
            throw Error.Conflict("DeleteAccount.NotSure",
                "Are you sure you want to remove your account, this cannot be undone.");
        
        User? user = await dbContext.Users.FirstOrDefaultAsync(b => b.Id == currentUserProvider.GetCurrentUserId(), token);
        
        if (user is null) throw Error.NotFound<User>();
        
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(token);

        await publishEndpoint.Publish<UserDeletedAccountEvent>(new ()
        {
            UserId = currentUserProvider.GetCurrentUserId()
        });
        
        return new Response() {};
    }
}

