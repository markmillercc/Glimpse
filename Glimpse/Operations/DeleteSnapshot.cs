using Glimpse.Db;
using MediatR.Endpoints;

namespace Glimpse.Operations
{
    [Endpoint(Method.Delete)]
    public class DeleteSnapshot
    {
        public class Command : Operation.IRequest<bool>
        {
            public int SnapshotId { get; set; }
        }

        public class CommandHandler : Operation.Handler<Command, bool>
        {
            private readonly GlimpseDbContext _db;
            public CommandHandler(GlimpseDbContext db)
            {
                _db = db;
            }

            protected override async Task<bool> DoOperation(Command request, CancellationToken cancellationToken)
            {
                var snapshot = await _db.Snapshots.FindAsync(request.SnapshotId, cancellationToken);
                if (snapshot == null)
                    return Error($"Cannot find snapshot with id={request.SnapshotId}");

                _db.Snapshots.Remove(snapshot);
                await _db.SaveChangesAsync(cancellationToken); ;

                return true;
            }
        }
    }
}
