using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Glimpse.Operations.Snapshots;

[Endpoint(Method.Get, "list/{profileid}", Group = "snapshots")]
public class GetSnapshotList
{
    public class Query : Operation.IRequest<IEnumerable<SnapshotModel>>
    {
        public int ProfileId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class QueryHandler : Operation.Handler<Query, IEnumerable<SnapshotModel>>
    {
        private readonly GlimpseDbContext _db;
        public QueryHandler(GlimpseDbContext db)
        {
            _db = db;
        }

        protected override async Task<IEnumerable<SnapshotModel>> DoOperation(Query request, CancellationToken cancellationToken)
        {
            var snapshots = await _db.Snapshots
                .Include(a => a.Entries).ThenInclude(a => a.Category)
                .Include(a => a.Entries).ThenInclude(a => a.Subject)
                .Where(a => a.ProfileId == request.ProfileId)
                .OrderBy(a => a.Start)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var resultList = new List<SnapshotModel>();
            var builder = new SnapshotBuilder();

            for (var i = 0; i < snapshots.Count; i++)
            {
                var snapshot = snapshots[i];
                var previousSnapshot = i == 0 ? null : snapshots[i - 1];

                var snapshotDetail = builder.Build(snapshot, previousSnapshot);

                resultList.Add(snapshotDetail);
            }

            return resultList;
        }

    }
}
