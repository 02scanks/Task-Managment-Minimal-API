using FluentValidation;
using MinimalTaskManagingAPI.Models.DTO;

namespace MinimalTaskManagingAPI.Validations.TaskValidations
{
    public class UpdateTaskValidation : AbstractValidator<UpdateTaskStatusDTO>
    {
        public UpdateTaskValidation()
        {
            RuleFor(model => model.TaskName).NotEmpty().WithMessage("Task name field must be filled out");
            RuleFor(model => model.Notes).NotEmpty().WithMessage("Notes field must be filled out");
            RuleFor(model => model.CompleteDate).NotEmpty().WithMessage("Completion date field must be filled out");
        }

    }
}
