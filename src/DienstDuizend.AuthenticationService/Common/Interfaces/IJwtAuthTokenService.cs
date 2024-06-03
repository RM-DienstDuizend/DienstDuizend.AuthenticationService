using System.Security.Claims;

namespace DienstDuizend.AuthenticationService.Common.Interfaces;

public interface IJwtAuthTokenService
{
    public string GenerateAccessToken(List<Claim> claims);
    public string GenerateAccessTokenWithRefreshToken(string refreshToken);
    public string GenerateRefreshToken(List<Claim> claims);
}