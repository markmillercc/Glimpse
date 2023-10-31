using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Glimpse.Operations
{
    [Endpoint(Method.Delete)]
    public class DeleteCategory
    {
        public class Command : Operation.IRequest<bool>
        {
            public int CategoryId { get; set; }
        }

        public class CommandHandler : Operation.Handler<Command, bool>
        {
            private readonly GlimpseDbContext _db;
            public CommandHandler(GlimpseDbContext db, IEnumerable<EndpointDataSource> ds)
            {
                var t = ds.ToList();
                var tt = ds.SelectMany(a => a.Endpoints).ToList();
                _db = db;
            }

            protected override async Task<bool> DoOperation(Command request, CancellationToken cancellationToken)
            {
                var category = await _db.Categories.FindAsync(request.CategoryId, cancellationToken);                
                if (category == null)
                    return Error($"Cannot find category with id={request.CategoryId}");

                if (await _db.Entries.AnyAsync(a => a.CategoryId == category.Id, cancellationToken))
                    return Error("Category is in use and cannot be deleted");

                _db.Categories.Remove(category);
                await _db.SaveChangesAsync(cancellationToken); ;

                return true;
            }
        }
    }
}
