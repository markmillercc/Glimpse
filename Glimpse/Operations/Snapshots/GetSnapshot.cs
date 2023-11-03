using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Glimpse.Operations.Snapshots;

[Endpoint(Method.Get, "get/{snapshotid}", Group = "snapshots")]
public class GetSnapshot
{
    public class Query : Operation.IRequest<SnapshotModel>
    {
        public int SnapshotId { get; set; }
    }

    public class QueryHandler : Operation.Handler<Query, SnapshotModel>
    {
        private readonly GlimpseDbContext _db;

        public QueryHandler(GlimpseDbContext db) => _db = db;

        protected override async Task<SnapshotModel> DoOperation(Query request, CancellationToken cancellationToken)
        {
            var snapshot = await _db.Snapshots
                .Include(a => a.Entries).ThenInclude(a => a.Category)
                .Include(a => a.Entries).ThenInclude(a => a.Subject)
                .FirstOrDefaultAsync(a => a.Id == request.SnapshotId, cancellationToken);

            if (snapshot == null)
                return Unprocessable($"Cannot find snapshot id={request.SnapshotId}");

            var previousSnapshot = await _db.Snapshots
                .Include(a => a.Entries)
                .Where(a => a.Start < snapshot.Start)
                .Where(a => a.ProfileId == snapshot.ProfileId)
                .OrderByDescending(a => a.Start)
                .FirstOrDefaultAsync(cancellationToken);

            var builder = new SnapshotBuilder();

            var snapshotDetail = builder.Build(snapshot, previousSnapshot);

            return snapshotDetail;
        }
    }
}

