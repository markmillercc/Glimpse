using System.ComponentModel.DataAnnotations;

namespace Glimpse.Plumbing;

public class ModelStateValidationEndpointFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var errors = new List<string>();
        var args = context.Arguments.ToList();
        foreach (var arg in args)
        {
            var validator = new ValidationContext(arg);
            var validationErrors = new List<ValidationResult>();

            if (!Validator.TryValidateObject(arg, validator, validationErrors, true))
            {
                errors.AddRange(validationErrors
                    .Select(a => a.ErrorMessage)
                    .ToList());
            }
        }

        if (errors.Count > 0)
        {
            return Results.BadRequest(new { Errors = errors, IsValid = false });
        }

        return await next(context);
    }
}
