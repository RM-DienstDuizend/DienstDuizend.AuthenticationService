
using DienstDuizend.AuthenticationService.Features.Authentication.Domain.Enums;
using DienstDuizend.AuthenticationService.Features.Authentication.Domain.ValueObjects;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Domain;

public class User
{
    public Guid Id { get; set; }
    public Email Email { get; set; }
    public string HashedPassword { get; set; }
    public string? TwoFactorKey { get; set; }
    public string? RecoverySentence { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsPermanentlyBlocked { get; set; }
    public int FailedAttempts { get; set; }
    public string? LockoutRemovalKey { get; set; }

    public Role Role { get; set; } = Role.User;

}