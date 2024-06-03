using DienstDuizend.AuthenticationService.Common.Dto;
using DienstDuizend.AuthenticationService.Common.Extensions;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain.Enums;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain.ValueObjects;
using DienstDuizend.AuthenticationService.Infrastructure.Persistence;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.GetAllUsersWithPagination;

[Handler]
public static partial class GetAllUsersWithPagination
{
    public record Query(int PageSize = 100, int PageIndex = 1);

    public record Response(Guid Id, Email Email, DateTime LastLogin, AccessStatus AccessStatus, Role Role);

    private static async ValueTask<PaginationResult<Response>> HandleAsync(
        Query request,
        ApplicationDbContext dbContext,
        CancellationToken token)
    {
        var users = await dbContext.Users
            .Paginate(request.PageIndex, request.PageSize)
            .Select(x =>

                new Response(
                    x.Id,
                    x.Email,
                    x.LastLogin,
                    GetCorrectAccessStatus(x),
                    x.Role
                    )
            )
            .ToListAsync(token);

        return new PaginationResult<Response>
        {
            Data = users,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalRecords = await dbContext.Users.CountAsync(token)
        };

        
    }

    private static AccessStatus GetCorrectAccessStatus(User user)
    {
        if (user.IsPermanentlyBlocked) return AccessStatus.PermanentlyBlocked;

        if (user.LockoutRemovalKey is not null) return AccessStatus.LockedOut;

        return AccessStatus.Active;
        
    }
}

public enum AccessStatus
{
    Active,
    LockedOut,
    PermanentlyBlocked
}