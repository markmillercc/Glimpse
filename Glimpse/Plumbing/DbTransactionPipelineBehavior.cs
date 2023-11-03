using Glimpse.Db;
using MediatR;

namespace Glimpse.Plumbing;

public class DbTransactionPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly GlimpseDbContext _db;
    private readonly ILogger<DbTransactionPipelineBehavior<TRequest, TResponse>> _logger;
    public DbTransactionPipelineBehavior(GlimpseDbContext db, ILogger<DbTransactionPipelineBehavior<TRequest, TResponse>> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            await _db.BeginTransactionAsync(cancellationToken);

            var reponse = await next();

            await _db.CommitTransactionAsync(cancellationToken);

            return reponse;
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, ex.Message);
            await _db.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
