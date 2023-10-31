using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Glimpse.Operations
{
    [Endpoint(Method.Post)]
    public class SaveSnapshot
    {
        public class Command : Operation.IRequest<int>
        {
            public int? SnapshotId { get; set; }
            public int ProfileId { get; set; }
            [Required]
            public DateTime? Start { get; set; }
            [Required]           
            public DateTime? End { get; set; }
        }

        public class Handler : Operation.Handler<Command, int>
        {
            private readonly GlimpseDbContext _db;
            public Handler(GlimpseDbContext db)
            {
                _db = db;
            }

            protected override async Task<int> DoOperation(Command request, CancellationToken cancellationToken)
            {
                if (request.End < request.Start)
                    return Error("End date cannot be before start date");

                Db.Entities.Snapshot snapshot;
                if (request.SnapshotId.HasValue)
                {
                    snapshot = await _db.Snapshots
                        .Where(a => a.Id == request.SnapshotId && a.ProfileId == request.ProfileId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (snapshot == null)
                        return Error($"Cannot find snapshot with id={request.SnapshotId} and profile id={request.ProfileId}");

                    snapshot.SetDates(request.Start.Value, request.End.Value);                    
                }
                else
                {
                    var profile = await _db.Profiles.FindAsync(request.ProfileId, cancellationToken);
                    if (profile == null)
                        return Error($"Cannot find profile with id={request.ProfileId}");

                    snapshot = new Db.Entities.Snapshot(profile, request.Start.Value, request.End.Value);
                    await _db.Snapshots.AddAsync(snapshot, cancellationToken);
                }                
                
                await _db.SaveChangesAsync(cancellationToken);

                return snapshot.Id;
            }
        }
    }
}




//public async Task<int> Handle(Command request, CancellationToken cancellationToken)
//{
//    var result = new ValidatedResult<int>();

//    if (!request.Start.HasValue || !request.End.HasValue)
//        return result.Failure("Start and end date are required");

//    if (request.End < request.Start)
//        return result.Failure("End date must come after start date");

//    var profile = await _db.Profiles.FindAsync(request.ProfileId, cancellationToken);
//    if (profile == null)
//        return result.Failure($"Cannot find profile with id={request.ProfileId}");

//    var snapshot = new Db.Entities.Snapshot(profile, request.Start.Value, request.End.Value);

//    await _db.Snapshots.AddAsync(snapshot, cancellationToken);
//    await _db.SaveChangesAsync(cancellationToken);

//    return result.Success(snapshot.Id);
//}
