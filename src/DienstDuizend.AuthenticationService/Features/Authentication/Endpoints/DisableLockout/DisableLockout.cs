using Microsoft.AspNetCore.Mvc;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.DisableLockout;

[ApiController, Route("/disable-lockout")]
public class DisableLockoutEndpoint(DisableLockout.Handler handler) : ControllerBase
{
    [HttpPost]
    public async Task<DisableLockout.Response> HandleAsync(
        [FromBody] DisableLockout.Command request,
        CancellationToken cancellationToken = new()
    ) => await handler.HandleAsync(request, cancellationToken);
}