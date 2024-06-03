using Microsoft.AspNetCore.Mvc;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.Register;

[ApiController, Route("/register")]
public class RegisterEndpoint(Register.Handler handler) : ControllerBase
{
    [HttpPost]
    public async Task<Register.Response> HandleAsync(
        [FromBody] Register.Command request,
        CancellationToken cancellationToken = new()
    ) => await handler.HandleAsync(request, cancellationToken);
}