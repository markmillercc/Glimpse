using Glimpse.Operations;

namespace Glimpse.Plumbing;

public class OperationValidationEndpointFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var response = await next(context);                

        if (response is Operation.IResponse op)
        {
            if (!op.IsValid)
                return Results.UnprocessableEntity(new { op.Errors });

            return Results.Ok(op.Result);
        }

        return Results.Ok(response);
    }
}
