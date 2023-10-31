using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Glimpse.Operations
{
    [Endpoint(Method.Delete)]
    public class DeleteProfile
    {
        public class Command : Operation.IRequest<bool>
        {
            public int ProfileId { get; set; }
        }

        public class CommandHandler : Operation.Handler<Command, bool>
        {
            private readonly GlimpseDbContext _db;
            public CommandHandler(GlimpseDbContext db) => _db = db;

            protected override async Task<bool> DoOperation(Command request, CancellationToken cancellationToken)
            {
                var profile = await _db.Profiles
                    .Include(a => a.Subjects)
                    .Include(a => a.Categories)
                    .Include(a => a.Snapshots)
                    .FirstOrDefaultAsync(a => a.Id == request.ProfileId, cancellationToken);

                if (profile == null)
                    return Error($"Cannot find profile with id={request.ProfileId}");

                if (profile.Snapshots.Any() || profile.Subjects.Any() || profile.Categories.Any())
                    return Error("Cannot delete profile because it still has child items");

                _db.Profiles.Remove(profile);
                await _db.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }
}
