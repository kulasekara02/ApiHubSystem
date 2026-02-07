using FluentValidation;

namespace ApiHub.Application.Features.Webhooks.Commands;

public class UpdateWebhookCommandValidator : AbstractValidator<UpdateWebhookCommand>
{
    public UpdateWebhookCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Webhook ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required.")
            .MaximumLength(500).WithMessage("URL must not exceed 500 characters.")
            .Must(BeAValidUrl).WithMessage("URL must be a valid HTTPS URL.");

        RuleFor(x => x.Secret)
            .MinimumLength(16).WithMessage("Secret must be at least 16 characters.")
            .MaximumLength(256).WithMessage("Secret must not exceed 256 characters.")
            .When(x => !string.IsNullOrEmpty(x.Secret));

        RuleFor(x => x.Events)
            .NotEmpty().WithMessage("At least one event must be selected.");
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && uriResult.Scheme == Uri.UriSchemeHttps;
    }
}
