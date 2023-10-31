using Glimpse.Operations;

namespace Glimpse.Plumbing;

public class OperationValidationEndpointFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var response = await next(context);

        if (response is Operation.IResponse op && !op.IsValid)
            return Results.BadRequest(response);
        
        return Results.Ok(response);
    }
}
