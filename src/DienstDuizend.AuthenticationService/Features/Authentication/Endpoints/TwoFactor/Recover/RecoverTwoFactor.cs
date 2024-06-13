using DienstDuizend.AuthenticationService.Features.Authentication.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.TwoFactor.Recover;

[ApiController, Route("/2fa/recover"), Authorize]
public class RecoverTwoFactorEndpoint(TwoFactorRecover.Handler handler) : ControllerBase
{
    [HttpPost]
    public async Task<TwoFactorRecover.Response> HandleAsync(
        [FromBody] TwoFactorRecover.Command request,
        CancellationToken cancellationToken = new()
    ) => await handler.HandleAsync(request, cancellationToken);
}