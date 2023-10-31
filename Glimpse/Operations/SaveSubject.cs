using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Glimpse.Operations
{
    [Endpoint(Method.Post, "fudge/subjects/add", Group = "Mygrrr")]
    public class SaveSubject
    {
        public class Command : Operation.IRequest<int>
        {
            public int? SubjectId { get; set; }
            public int ProfileId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class Handler : Operation.Handler<Command, int>
        {
            private readonly GlimpseDbContext _db;
            public Handler(GlimpseDbContext db) => _db = db;

            protected override async Task<int> DoOperation(Command request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return Error("Name is required");

                var profile = await _db.Profiles
                    .Include(a => a.Subjects)
                    .FirstOrDefaultAsync(a => a.Id == request.ProfileId, cancellationToken);

                if (profile == null)
                    return Error($"Cannot find profile id={request.ProfileId}");

                var subjects = profile.Subjects.Where(a => request.SubjectId == null || a.Id != request.SubjectId.Value).ToList();
                if (subjects.Any(a => a.Name.ToLower() == request.Name.ToLower()))
                    return Error($"Profile Id={profile.Id} already has a subject named {request.Name}");

                Db.Entities.Subject subject;
                if (request.SubjectId.HasValue)
                {
                    subject = profile.Subjects.FirstOrDefault(a => a.Id == request.SubjectId.Value);
                    if (subject == null)
                        return Error($"Unable to find subject with id={request.SubjectId.Value} and profile id={profile.Id}");

                    subject.Name = request.Name;
                    subject.Description = request.Description;
                }
                else
                {
                    subject = new Db.Entities.Subject(profile, request.Name, request.Description);
                    await _db.Subjects.AddAsync(subject, cancellationToken);
                }                
                
                await _db.SaveChangesAsync(cancellationToken);
                return subject.Id;
            } 
        }
    }
}




//public async Task<IValidatedResult<int>> Handle(Command request, CancellationToken cancellationToken)
//{
//    var result = new ValidatedResult<int>();

//    if (string.IsNullOrWhiteSpace(request.Name))
//        return result.Failure("Name is required");

//    var profile = await _db.Profiles
//        .Include(a => a.Subjects)
//        .FirstOrDefaultAsync(a => a.Id == request.ProfileId, cancellationToken);

//    if (profile == null)
//        return result.Failure($"Cannot find profile id={request.ProfileId}");

//    if (profile.Subjects.Any(a => a.Name.ToLower() == request.Name.ToLower()))
//        return result.Failure($"Profile Id={profile.Id} already has a subject named {request.Name}");

//    var subject = new Db.Entities.Subject(profile, request.Name, request.Description);

//    await _db.Subjects.AddAsync(subject, cancellationToken);
//    await _db.SaveChangesAsync(cancellationToken);

//    return result.Success(subject.Id);
//}