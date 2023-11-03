using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Glimpse.Operations.Profiles;

[Endpoint(Method.Get, "get/{profileid}", Group = "profiles")]
public class GetProfile
{
    public class Query : Operation.IRequest<ProfileModel>
    {
        public int ProfileId { get; set; }
    }
    public class ProfileModel
    {
        public int ProfileId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<SnapshotModel> Snapshots { get; set; }
        public IReadOnlyDictionary<int, string> Subjects { get; set; }
        public IReadOnlyDictionary<int, string> Categories { get; set; }

    }
    public class SnapshotModel
    {
        public int SnapshotId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class QueryHandler : Operation.Handler<Query, ProfileModel>
    {
        private readonly GlimpseDbContext _db;
        public QueryHandler(GlimpseDbContext db) => _db = db;

        protected override async Task<ProfileModel> DoOperation(Query request, CancellationToken cancellationToken)
        {
            var profile = await _db.Profiles
                .Include(a => a.Snapshots)
                .Include(a => a.Subjects)
                .Include(a => a.Categories)
                .FirstOrDefaultAsync(a => a.Id == request.ProfileId, cancellationToken);

            if (profile == null)
                return Unprocessable($"Cannot find profile with id={request.ProfileId}");

            return new ProfileModel
            {
                ProfileId = profile.Id,
                Name = profile.Name,
                Description = profile.Description,
                Snapshots = profile.Snapshots.Select(snap => new SnapshotModel
                {
                    SnapshotId = snap.Id,
                    Start = snap.Start,
                    End = snap.End,
                }),
                Subjects = profile.Subjects.ToDictionary(a => a.Id, a => a.Name),
                Categories = profile.Categories.ToDictionary(a => a.Id, a => a.Name)
            };
        }
    }
}

