using Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        if (!_validators.Any())
            return await next(ct);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .Where(v => !v.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0) return await next(ct);

        var errorsByField = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var error = Error.Validation(errorsByField);

        return CreateFailureResult(error);
    }

    private static TResponse CreateFailureResult(Error error)
    {
        return (dynamic)CreateTypedFailure((dynamic)error, default(TResponse)!);
    }

    private static Result CreateTypedFailure(Error error, Result _)
    {
        return Result.Failure(error);
    }

    private static Result<T> CreateTypedFailure<T>(Error error, Result<T> _)
    {
        return Result<T>.Failure(error);
    }
}