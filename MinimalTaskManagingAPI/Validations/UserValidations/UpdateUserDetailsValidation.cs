using FluentValidation;
using MinimalTaskManagingAPI.Models.DTO;

namespace MinimalTaskManagingAPI.Validations.UserValidations
{
    public class UpdateUserDetailsValidation : AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserDetailsValidation()
        {
            RuleFor(model => model.Username)
                .NotEmpty().WithMessage("Username field must be filled out");

            RuleFor(model => model.OldPassword)
                .NotEmpty().WithMessage("Old password field must be filled out");

            RuleFor(model => model.NewPassword)
                .Must((model, newPassword) => BeDifferentPasswords(model.OldPassword, newPassword))
                .WithMessage("The new password should not be the same as the old password")
                .NotEmpty().WithMessage("New password field must be filled out");
        }

        private bool BeDifferentPasswords(string oldPassword, string newPassword)
        {
            return oldPassword.ToLower() != newPassword.ToLower();
        }
    }
}
