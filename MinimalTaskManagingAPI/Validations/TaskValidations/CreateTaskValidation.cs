using FluentValidation;
using MinimalTaskManagingAPI.Models.DTO;

namespace MinimalTaskManagingAPI.Validations.TaskValidations
{
    public class CreateTaskValidation : AbstractValidator<CreateTaskDTO>
    {
        public CreateTaskValidation()
        {
            RuleFor(model => model.TaskName).NotEmpty().WithMessage("Task name field must be filled out");
            RuleFor(model => model.Notes).NotEmpty().WithMessage("Task notes field must be filled out");
            RuleFor(model => model.CompleteDate).Must(BeInFuture).WithMessage("Completion date must be in the future");
        }

        private bool BeInFuture(DateTime dateTime) 
        {
            return dateTime > DateTime.Now;
        }
    }
}
