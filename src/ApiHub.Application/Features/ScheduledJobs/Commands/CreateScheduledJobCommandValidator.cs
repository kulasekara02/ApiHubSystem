using FluentValidation;

namespace ApiHub.Application.Features.ScheduledJobs.Commands;

public class CreateScheduledJobCommandValidator : AbstractValidator<CreateScheduledJobCommand>
{
    public CreateScheduledJobCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.ConnectorId)
            .NotEmpty().WithMessage("Connector is required");

        RuleFor(x => x.Endpoint)
            .NotEmpty().WithMessage("Endpoint is required")
            .MaximumLength(500).WithMessage("Endpoint cannot exceed 500 characters");

        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Method is required")
            .Must(m => new[] { "GET", "POST", "PUT", "PATCH", "DELETE" }.Contains(m.ToUpper()))
            .WithMessage("Method must be GET, POST, PUT, PATCH, or DELETE");

        RuleFor(x => x.CronExpression)
            .NotEmpty().WithMessage("Cron expression is required")
            .MaximumLength(100).WithMessage("Cron expression cannot exceed 100 characters");
    }
}
