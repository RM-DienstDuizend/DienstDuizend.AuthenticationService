namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.Login;

[ApiController, Route("/login")]
public class LoginEndpoint(Login.Handler handler) : ControllerBase
{
    [HttpPost]
    public async Task<Login.Response> HandleAsync(
        [FromBody] Login.Command request,
        CancellationToken cancellationToken = new()
    ) => await handler.HandleAsync(request, cancellationToken);
}