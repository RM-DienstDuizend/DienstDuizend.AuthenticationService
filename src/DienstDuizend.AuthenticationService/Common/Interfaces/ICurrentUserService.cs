namespace DienstDuizend.AuthenticationService.Common.Interfaces;

public interface ICurrentUserProvider
{
    public Guid GetCurrentUserId();
}