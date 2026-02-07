using FluentValidation;

namespace ApiHub.Application.Features.Templates.Commands;

public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    private static readonly string[] ValidMethods = { "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS" };

    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("HTTP method is required.")
            .Must(m => ValidMethods.Contains(m.ToUpperInvariant()))
            .WithMessage("Method must be a valid HTTP method (GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS).");

        RuleFor(x => x.Endpoint)
            .NotEmpty().WithMessage("Endpoint is required.")
            .MaximumLength(500).WithMessage("Endpoint must not exceed 500 characters.");

        RuleFor(x => x.Headers)
            .Must(BeValidJsonOrNull).WithMessage("Headers must be valid JSON.");

        RuleFor(x => x.Body)
            .Must(BeValidJsonOrNull).WithMessage("Body must be valid JSON.");

        RuleFor(x => x.QueryParams)
            .Must(BeValidJsonOrNull).WithMessage("Query parameters must be valid JSON.");
    }

    private bool BeValidJsonOrNull(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
