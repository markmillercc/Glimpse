using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Glimpse.Operations.Subjects;

[Endpoint(Method.Delete, "delete/{subjectid}", Group = "subjects")]
public class DeleteSubject
{
    public class Command : Operation.IRequest<bool>
    {
        public int SubjectId { get; set; }
    }

    public class CommandHandler : Operation.Handler<Command, bool>
    {
        private readonly GlimpseDbContext _db;
        public CommandHandler(GlimpseDbContext db) => _db = db;

        protected override async Task<bool> DoOperation(Command request, CancellationToken cancellationToken)
        {
            var subject = await _db.Subjects.FindAsync(request.SubjectId, cancellationToken);
            if (subject == null)
                return Unprocessable($"Cannot find subject with id={request.SubjectId}");

            if (await _db.Entries.AnyAsync(a => a.SubjectId == subject.Id, cancellationToken))
                return Unprocessable("Cannot delete subject while tied to entries");

            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

