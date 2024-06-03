using DienstDuizend.AuthenticationService.Common.Dto;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.GetAllUsersWithPagination;

[ApiController, Route("/users")]
[Authorize(Roles = nameof(Role.Administrator))]
public class GetAllUsersWithPaginationEndpoint(GetAllUsersWithPagination.Handler handler) : ControllerBase
{
    [HttpGet]
    public async Task<PaginationResult<GetAllUsersWithPagination.Response>> HandleAsync(
        [FromQuery] GetAllUsersWithPagination.Query request,
        CancellationToken cancellationToken = new()
    ) => await handler.HandleAsync(request, cancellationToken);
}