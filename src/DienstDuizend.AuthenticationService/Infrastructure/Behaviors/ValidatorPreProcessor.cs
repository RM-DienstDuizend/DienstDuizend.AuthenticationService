using FluentValidation;
using Immediate.Handlers.Shared;

namespace DienstDuizend.AuthenticationService.Infrastructure.Behaviors;

public sealed class ValidatorPreProcessor<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : Behavior<TRequest, TResponse>
{
    private readonly IReadOnlyCollection<IValidator<TRequest>> _validators = validators.ToList();

    public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        // FluentValidation validation
        foreach (var validator in _validators)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken); // Throws ValidationException on failure
        }

        return await Next(request, cancellationToken);
    }
}