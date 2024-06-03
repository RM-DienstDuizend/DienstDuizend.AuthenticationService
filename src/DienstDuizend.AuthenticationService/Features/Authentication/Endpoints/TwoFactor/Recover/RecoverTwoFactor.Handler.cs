using DienstDuizend.AuthenticationService.Common.Interfaces;
using DienstDuizend.AuthenticationService.Features.Authentication.TwoFactor;
using DienstDuizend.AuthenticationService.Infrastructure.Exceptions;
using DienstDuizend.AuthenticationService.Infrastructure.Persistence;
using DienstDuizend.AuthenticationService.Common.Extensions;
using Google.Authenticator;
using NanoidDotNet;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.TwoFactor.Recover;

[Handler]
public static partial class TwoFactorRecover
{
    public record Command(string? RecoverySentence);

    public record Response(
        bool Enabled,
        string? TwoFactorAuthenticationKey,
        string? TwoFactorAuthenticationQrImage,
        string? RecoverySentence
    );


    private static async ValueTask<Response> HandleAsync(
        Command request,
        ApplicationDbContext dbContext,
        ICurrentUserProvider currentUserProvider,
        IConfiguration configuration,
        TwoFactorAuthenticator tfa,
        CancellationToken token)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserProvider.GetCurrentUserId(), token);

        if (user.TwoFactorKey is null)
            throw Error.Conflict(
                "User.TwoFactorAuthenticationDisabled",
                "Your two factor authentication is currently disabled."
            );

        if (user.RecoverySentence.ToLower() != request.RecoverySentence)
        {
            throw Error.Conflict(
                "User.InvalidRecoverySentence",
                "The given recovery sentence is invalid"
            );
        }

        var generatedKey = await Nanoid.GenerateAsync(size: 16);
        var randomRecoveryPhrase = Random.Shared
            .GetItems(TwoFactoryRecoveryWords.RecoveryWordList, 5)
            .Join("-");

        SetupCode setupInfo =
            tfa.GenerateSetupCode(
                "DienstDuizend",
                user.Email.ToString(),
                generatedKey,
                false,
                3
            );

        user.TwoFactorKey = generatedKey;
        user.RecoverySentence = randomRecoveryPhrase;
        await dbContext.SaveChangesAsync(token);

        return new Response(
            true,
            setupInfo.ManualEntryKey,
            setupInfo.QrCodeSetupImageUrl,
            randomRecoveryPhrase
        );
    }
}