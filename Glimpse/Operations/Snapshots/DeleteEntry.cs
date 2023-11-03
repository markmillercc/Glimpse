using Glimpse.Db;
using MediatR.Endpoints;

namespace Glimpse.Operations.Snapshots;

[Endpoint(Method.Delete, "entries/delete/{entryid}", Group = "snapshots")]
public class DeleteEntry
{
    public class Command : Operation.IRequest<bool>
    {
        public int EntryId { get; set; }
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
            var entry = await _db.Entries.FindAsync(request.EntryId, cancellationToken);

            if (entry == null)
                return Unprocessable($"Cannot find entry with id={request.EntryId}");

            _db.Entries.Remove(entry);
            await _db.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
