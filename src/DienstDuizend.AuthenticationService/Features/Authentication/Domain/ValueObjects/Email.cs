using System.Net.Mail;
using Vogen;

namespace DienstDuizend.AuthenticationService.Features.Authentication.Domain.ValueObjects;

[ValueObject<string>]
public partial struct Email
{
    private static Validation Validate(string input)
    {
        try
        {
            var emailAddress = new MailAddress(input);
            return Validation.Ok;
        }
        catch
        {
            return Validation.Invalid("Invalid Email Address");
        }
    }
    
    private static string NormalizeInput(string input) => input.ToLower().Trim();
}