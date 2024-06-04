
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DienstDuizend.AuthenticationService.Common.Interfaces;
using DienstDuizend.AuthenticationService.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DienstDuizend.AuthenticationService.Infrastructure.Services;


public class JwtAuthSettings
{
    public int AccessTokenExpirationTimeInMinutes { get; set; }
    public string AccessTokenSecret { get; set; }
    
    public int RefreshTokenExpirationTimeInMinutes { get; set; }
    public string RefreshTokenSecret { get; set; }   
}

public class JwtAuthTokenService(IOptions<JwtAuthSettings> jwtSettings) : IJwtAuthTokenService
{
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
    private readonly JwtAuthSettings _jwtSettings = jwtSettings.Value;

    public string GenerateAccessToken(List<Claim> claims)
    {
        return Generate(claims, _jwtSettings.AccessTokenSecret,
            DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationTimeInMinutes));
    }

    public string GenerateAccessTokenWithRefreshToken(string refreshToken)
    {
        ClaimsPrincipal claimsPrinciple = _jwtSecurityTokenHandler
            .ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.RefreshTokenSecret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

        var claims = claimsPrinciple.Claims.ToList();

        return GenerateAccessToken(claims);
    }

    public string GenerateRefreshToken(List<Claim> claims)
    {
        return Generate(claims, _jwtSettings.RefreshTokenSecret,
            DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationTimeInMinutes));
    }

    private string Generate(List<Claim> claims, string secret, DateTime dateTime)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = dateTime,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                , SecurityAlgorithms.HmacSha256Signature)
        };
        var token = _jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
        return  _jwtSecurityTokenHandler.WriteToken(token);
    }
}
