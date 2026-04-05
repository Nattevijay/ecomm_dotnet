using Backend.DTOs;
using FluentValidation;

namespace Backend.Validators
{
    // ════════════════════════════════════════════════════════════════════════
    // AUTH VALIDATORS (FluentValidation)
    // ════════════════════════════════════════════════════════════════════════
    // FluentValidation runs BEFORE the controller action executes.
    // If validation fails → returns 400 Bad Request automatically.
    // You never need to write if (string.IsNullOrEmpty(dto.Email)) manually.
    //
    // REGISTER in Program.cs:
    //   builder.Services.AddFluentValidationAutoValidation();
    //   builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
    // ════════════════════════════════════════════════════════════════════════

    // ── Register Validator ───────────────────────────────────────────────
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            // NAME
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("Name is required")
                .MinimumLength(2)
                    .WithMessage("Name must be at least 2 characters")
                .MaximumLength(100)
                    .WithMessage("Name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                    .WithMessage("Name can only contain letters and spaces");

            // EMAIL
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage("Email is required")
                .EmailAddress()
                    .WithMessage("Please provide a valid email address")
                .MaximumLength(200)
                    .WithMessage("Email cannot exceed 200 characters");

            // PASSWORD — strong password rules
            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("Password is required")
                .MinimumLength(8)
                    .WithMessage("Password must be at least 8 characters")
                .MaximumLength(100)
                    .WithMessage("Password cannot exceed 100 characters")
                .Matches(@"[A-Z]")
                    .WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]")
                    .WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]")
                    .WithMessage("Password must contain at least one number")
                .Matches(@"[^a-zA-Z0-9]")
                    .WithMessage("Password must contain at least one special character (!@#$%...)");

            // ROLE
            RuleFor(x => x.Role)
                .NotEmpty()
                    .WithMessage("Role is required")
                .Must(role => role == "Customer" || role == "Seller")
                    .WithMessage("Role must be either 'Customer' or 'Seller'");
        }
    }

    // ── Login Validator ──────────────────────────────────────────────────
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage("Email is required")
                .EmailAddress()
                    .WithMessage("Please provide a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("Password is required");
        }
    }

    // ── Update Profile Validator ─────────────────────────────────────────
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("Name is required")
                .MinimumLength(2)
                    .WithMessage("Name must be at least 2 characters")
                .MaximumLength(100)
                    .WithMessage("Name cannot exceed 100 characters");

            // Phone is optional but if provided, must be valid
            RuleFor(x => x.Phone)
                .Matches(@"^\+?[0-9]{7,15}$")
                    .WithMessage("Please provide a valid phone number")
                .When(x => !string.IsNullOrEmpty(x.Phone));
        }
    }

    // ── Change Password Validator ────────────────────────────────────────
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                    .WithMessage("Current password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                    .WithMessage("New password is required")
                .MinimumLength(8)
                    .WithMessage("New password must be at least 8 characters")
                .Matches(@"[A-Z]")
                    .WithMessage("New password must contain at least one uppercase letter")
                .Matches(@"[a-z]")
                    .WithMessage("New password must contain at least one lowercase letter")
                .Matches(@"[0-9]")
                    .WithMessage("New password must contain at least one number")
                .Matches(@"[^a-zA-Z0-9]")
                    .WithMessage("New password must contain at least one special character")
                .NotEqual(x => x.CurrentPassword)
                    .WithMessage("New password must be different from current password");
        }
    }
}