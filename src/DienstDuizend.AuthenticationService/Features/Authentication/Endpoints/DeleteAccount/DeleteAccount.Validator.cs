using FluentValidation;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.DeleteAccount;

public class DeleteAccountValidator : AbstractValidator<DeleteAccount.Command>
{
    public DeleteAccountValidator()
    {
    }       
}
