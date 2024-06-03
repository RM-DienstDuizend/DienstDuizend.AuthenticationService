namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.RefreshToken;

[ApiController, Route("/refresh-token")]
public class RefreshTokenEndpoint(RefreshToken.Handler handler) : ControllerBase
{
    [HttpPost]
    public async Task<RefreshToken.Response> HandleAsync(
        CancellationToken cancellationToken = new()
    ) => await handler.HandleAsync(new RefreshToken.Command(), cancellationToken);
}