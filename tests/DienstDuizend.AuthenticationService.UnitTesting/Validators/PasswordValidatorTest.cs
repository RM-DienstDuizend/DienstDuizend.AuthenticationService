using DienstDuizend.AuthenticationService.Features.Authentication.Domain.ValueObjects;
using DienstDuizend.AuthenticationService.Features.Authentication.Endpoints.Register;
using FluentValidation.TestHelper;

namespace DienstDuizend.AuthenticationService.UnitTesting.Validators
{
    public class PasswordValidatorTest
    {
        private readonly RegisterValidator _validator;

        public PasswordValidatorTest()
        {
            _validator = new RegisterValidator();
        }

        [Theory]
        [InlineData("", "Your password cannot be empty.")]
        [InlineData("short", "Your password length must be at least 12 characters.")]
        [InlineData("S3cur3Pa$$w0rd-F0r-Dummy-Syst3m_Th@t-1s-N3v3r-Us3d!!", "Your password length must not exceed 128 characters.")]
        [InlineData("nouppercase123!", "Your password must contain at least one uppercase letter.")]
        [InlineData("NOLOWERCASE123!", "Your password must contain at least one lowercase letter.")]
        [InlineData("NoNumbers!", "Your password must contain at least one number.")]
        [InlineData("NoSpecialChars123", "Your password must contain at least one (!? *.).")]
        public void Should_Have_Validation_Error_For_Invalid_Passwords(string password, string expectedErrorMessage)
        {
            var command = new Register.Command(
                Email: Email.From("test@example.com"), 
                FirstName: "FirstName",
                LastName: "LastName",
                Password: password,
                ConfirmPassword: password
            );
            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage(expectedErrorMessage);
        }

        [Fact]
        public void FailFast()
        {
            true.Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Have_Validation_Error_For_Valid_Password()
        {
            var command = new Register.Command(
                Email: Email.From("test@example.com"), 
                FirstName: "FirstName",
                LastName: "LastName",
                Password: "ValidPassword123!",
                ConfirmPassword: "ValidPassword123!"
            );
            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Have_Validation_Error_When_Password_And_Email_Are_Same()
        {
            var command = new Register.Command(
                Email: Email.From("test@example.com"), 
                FirstName: "FirstName",
                LastName: "LastName",
                Password: "test@example.com",
                ConfirmPassword: "test@example.com"
            );
            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Your password and email cannot be the same value.");
        }

        [Fact]
        public void Should_Have_Validation_Error_When_Password_And_ConfirmPassword_Do_Not_Match()
        {
            var command = new Register.Command(
                Email: Email.From("test@example.com"), 
                FirstName: "FirstName",
                LastName: "LastName",
                Password: "ValidPassword123!",
                ConfirmPassword: "DifferentPassword123!"
            );
            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Your password and confirmed password are not the same value.");
        }

        [Fact]
        public void Should_Have_Validation_Error_For_Exceeding_MaxDuplicateChars()
        {
            var command = new Register.Command(
                Email: Email.From("test@example.com"), 
                FirstName: "FirstName",
                LastName: "LastName",
                Password: "AaaaaaaaaBbCc123!",
                ConfirmPassword: "AaaaaaaaaBbCc123!"
            );
            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}
