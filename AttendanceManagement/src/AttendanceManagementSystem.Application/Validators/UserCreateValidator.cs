using AttendanceManagementSystem.Application.DTOs.Auth;
using FluentValidation;


namespace AttendanceManagementSystem.Application.Validators
{
    public class UserCreateValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("UserName is required")
                .Length(3, 20)
                .WithMessage("UserName must be between 3 and 20 characters long");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .Length(8, 20)
                .WithMessage("Password must be between 8 and 20 characters long");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");

            RuleFor(x => x.FirstName)
               .Length(2, 50)
               .When(x => !string.IsNullOrWhiteSpace(x.FirstName))
               .WithMessage("FirstName must be between 2 and 50 characters long");

            RuleFor(x => x.LastName)
                .Length(2, 50)
                .When(x => !string.IsNullOrWhiteSpace(x.LastName))
                .WithMessage("LastName must be between 2 and 50 characters long");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Invalid phone number format");
        }
    }
}