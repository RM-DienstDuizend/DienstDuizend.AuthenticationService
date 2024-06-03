using DienstDuizend.AuthenticationService.Common.Interfaces;
using DienstDuizend.AuthenticationService.Features.Authentication.TwoFactor;
using DienstDuizend.AuthenticationService.Infrastructure.Exceptions;
using DienstDuizend.AuthenticationService.Infrastructure.Persistence;
using DienstDuizend.AuthenticationService.Common.Extensions;
using Google.Authenticator;
using NanoidDotNet;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.TwoFactor.Toggle;

[Handler]
public static partial class TwoFactorToggle
{
    public record Command(
        bool Enabled, 
        string? OneTimePassword
    );

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
        TwoFactorAuthenticator tfa,
        CancellationToken token)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserProvider.GetCurrentUserId(), token);

        // If Disabled
        if (!request.Enabled)
        {
            if (request.OneTimePassword is null)
                throw Error.Failure("User.MissingOTP", "Please provide a valid OTP code to disable 2fa.");
            
            if (!tfa.ValidateTwoFactorPIN(user.TwoFactorKey, request.OneTimePassword))
                throw Error.Failure("User.IncorrectOTP", "The given OTP code is invalid");

            user.TwoFactorKey = null;
            user.RecoverySentence = null;
            await dbContext.SaveChangesAsync(token);

            return new Response(
                false,
                null,
                null,
                null
            );
        }
        
        // If Enabled
        if (user.TwoFactorKey is not null)
        {
            throw Error.Forbidden("TwoFactorAuthentication.AlreadyEnabled",
                "2fa is already enabled, use the recover endpoint if 2fa needs to be re-setup.");
        }

        var generatedKey = await Nanoid.GenerateAsync(size: 21);
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