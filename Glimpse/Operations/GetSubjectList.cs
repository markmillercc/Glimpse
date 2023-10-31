using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Glimpse.Operations
{
    [Endpoint(Method.Get)]
    public class GetSubjectList
    {
        public class Query : Operation.IRequest<IEnumerable<SubjectModel>>
        {
            public int ProfileId { get; set; }
        }

        public class SubjectModel
        {
            public int Id { get; set; }
            public int ProfileId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class Handler : Operation.Handler<Query, IEnumerable<SubjectModel>>
        {
            private readonly GlimpseDbContext _db;

            public Handler(GlimpseDbContext db)
            {
                _db = db;
            }
            
            protected override async Task<IEnumerable<SubjectModel>> DoOperation(Query request, CancellationToken cancellationToken)
            {
                var profile = await _db.Profiles.Include(a => a.Subjects)
                    .FirstOrDefaultAsync(a => a.Id == request.ProfileId, cancellationToken);

                if (profile == null)
                    return Error($"Cannot find profile id={request.ProfileId}");

                return profile.Subjects.Select(subject => new SubjectModel
                {
                    Id = subject.Id,
                    ProfileId = subject.ProfileId,
                    Name = subject.Name,
                    Description = subject.Description
                }).ToList();
            }
        }
    }
}




//public async Task<IEnumerable<Subject>> Handle(Query request, CancellationToken cancellationToken)
//{
//    var subjects = new List<Db.Entities.Subject>();
//    if (request.SnapshotId.HasValue)
//    {
//        subjects = await _db.Entries
//            //.Where(e => e.ProfileId == request.ProfileId)
//            .Where(e => e.SnapshotId == request.SnapshotId.Value)
//            .Select(e => e.Subject)
//            .ToListAsync();
//    }
//    else
//    {
//        subjects = await _db.Subjects
//            .Where(a => a.ProfileId == request.ProfileId)
//            .ToListAsync();
//    }

//    return subjects.Select(subject => new Subject
//    {
//        Id = subject.Id,
//        ProfileId = subject.ProfileId,
//        Name = subject.Name,
//        Description = subject.Description
//    });
//}