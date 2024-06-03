using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.TwoFactor.Toggle;

[ApiController, Route("/2fa")]
[Authorize]
public class ToggleTwoFactorEndpoint(TwoFactorToggle.Handler handler) : ControllerBase
{
    [HttpPost]
    public async Task<TwoFactorToggle.Response> HandleAsync(
        [FromBody] TwoFactorToggle.Command request,
        CancellationToken cancellationToken = new()
    ) => await handler.HandleAsync(request, cancellationToken);
}