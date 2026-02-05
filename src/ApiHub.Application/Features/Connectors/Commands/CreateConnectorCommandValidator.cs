using FluentValidation;

namespace ApiHub.Application.Features.Connectors.Commands;

public class CreateConnectorCommandValidator : AbstractValidator<CreateConnectorCommand>
{
    public CreateConnectorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("Base URL is required.")
            .Must(BeAValidUrl).WithMessage("Base URL must be a valid URL.");

        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(1, 300).WithMessage("Timeout must be between 1 and 300 seconds.");

        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(0, 10).WithMessage("Max retries must be between 0 and 10.");

        RuleForEach(x => x.Endpoints).SetValidator(new CreateEndpointDtoValidator());
    }

    private bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class CreateEndpointDtoValidator : AbstractValidator<CreateEndpointDto>
{
    public CreateEndpointDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Endpoint name is required.")
            .MaximumLength(100).WithMessage("Endpoint name must not exceed 100 characters.");

        RuleFor(x => x.Path)
            .NotEmpty().WithMessage("Endpoint path is required.")
            .MaximumLength(500).WithMessage("Endpoint path must not exceed 500 characters.");
    }
}
