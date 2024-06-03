using DienstDuizend.AuthenticationService.Infrastructure.Exceptions;
using DienstDuizend.AuthenticationService.Infrastructure.Persistence;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.DisableLockout;

[Handler]
public static partial class DisableLockout
{
    public record Command(string LockoutKey);

    public record Response(string Message = "Successfully disabled lockout for user.");

    private static async ValueTask<Response> HandleAsync(
            Command request,
            ApplicationDbContext dbContext,
            CancellationToken token)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.LockoutRemovalKey == request.LockoutKey, token);
        
            if (user is null) throw Error.Failure("User.InvalidCode", "The given lockout code is invalid.");

            user.LockoutRemovalKey = null;
            user.FailedAttempts = 0;
            await dbContext.SaveChangesAsync(token);

            return new Response();
        }
    }

