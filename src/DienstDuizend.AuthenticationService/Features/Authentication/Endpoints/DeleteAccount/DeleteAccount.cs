using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.DeleteAccount;

[ApiController, Route("/delete-account")]
[Authorize]
public class DeleteAccountEndpoint(DeleteAccount.Handler handler) : ControllerBase
{
    [HttpDelete]
    public async Task<DeleteAccount.Response> HandleAsync(
        [FromBody] DeleteAccount.Command request,
        CancellationToken cancellationToken = new()
    ) => await handler.HandleAsync(request, cancellationToken);
}