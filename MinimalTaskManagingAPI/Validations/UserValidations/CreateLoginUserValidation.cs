using FluentValidation;
using MinimalTaskManagingAPI.Models.DTO;

namespace MinimalTaskManagingAPI.Validations.UserValidations
{
    public class CreateLoginUserValidation : AbstractValidator<UserDTO>
    {
        public CreateLoginUserValidation()
        {
            RuleFor(model => model.Username).NotEmpty().WithMessage("Username field must be filled out");
            RuleFor(model => model.Password).NotEmpty().WithMessage("Password field must be filled out");
        }
    }
}
