using DienstDuizend.AuthenticationService.Common.Extensions;
using FluentValidation;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.Register;

public class RegisterValidator : AbstractValidator<Register.Command>
{
    // DON'T FORGET TO MODIFY THIS ON THE FRONT END AS WELL
    public RegisterValidator()
    {
        RuleFor(u => u.Email).NotEmpty();
        RuleFor(u => u.FirstName).NotEmpty();
        RuleFor(u => u.LastName).NotEmpty();

        RuleFor(u => u.Password).NotEmpty().WithMessage("Your password cannot be empty.")
            .MinimumLength(12).WithMessage("Your password length must be at least 12 characters.")
            .MaximumLength(128).WithMessage("Your password length must not exceed 128 characters.")
            .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
            .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
            .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.")
            .Matches(@"[\!\?\*\.]+").WithMessage("Your password must contain at least one (!? *.).");
            //.IsNotContainedIn(CommonPasswordList.Values).WithMessage("Your password is too commonly used, please try something else.");
        
        
        RuleFor(user => user.Password)
            .MaxDuplicateChars(6)
            .NotEqual(user => user.Email.ToString())
            .WithMessage("Your password and email cannot be the same value.")
            .Equal(user => user.ConfirmPassword)
            .WithMessage("Your password and confirmed password are not the same value.");
    }       
}
