using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Glimpse.Operations.Snapshots;

[Endpoint(Method.Post, "entries/save", Group = "snapshots")]
public class SaveEntry
{
    public class Command : Operation.IRequest<int>
    {
        public int? EntryId { get; set; }
        public int SnapshotId { get; set; }
        public int SubjectId { get; set; }
        public int? CategoryId { get; set; }
        public Db.Entities.EntryType Type { get; set; }
        public string Description { get; set; }            
        [Range(1, int.MaxValue)]
        public decimal Quantity { get; set; }            
        public decimal Price { get; set; }
    }

    public class CommandHandler : Operation.Handler<Command, int>
    {
        private readonly GlimpseDbContext _db;
        public CommandHandler(GlimpseDbContext db) => _db = db;

        protected override async Task<int> DoOperation(Command request, CancellationToken cancellationToken)
        {
            if (request.Price == 0)
                return Unprocessable("Price cannot be zero");

            var snapshot = await _db.Snapshots
                .Include(a => a.Entries)
                .FirstOrDefaultAsync(a => a.Id == request.SnapshotId, cancellationToken);

            if (snapshot == null)
                return Unprocessable($"Cannot find snapshot with id={request.SnapshotId}");

            var subject = await _db.Subjects
                .Where(a => a.Id == request.SubjectId && a.ProfileId == snapshot.ProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            if (subject == null)
                return Unprocessable($"Cannot find subject with id={request.SubjectId} belonging to profile id={snapshot.ProfileId}");

            if (snapshot.Entries.Any(a => (request.EntryId == null || a.Id != request.EntryId) && a.SubjectId == subject.Id))
                return Unprocessable($"Subject '{subject.Name}' already exists on snapshot id={snapshot.Profile}");

            Db.Entities.Category category = null;
            if (request.CategoryId.HasValue)
            {
                category = await _db.Categories
                    .Where(a => a.Id == request.CategoryId && a.ProfileId == snapshot.ProfileId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (category == null)
                    return Unprocessable($"Cannot find category with id={request.CategoryId} belonging to profile id={snapshot.ProfileId}");
            }

            Db.Entities.Entry entry;
            if (request.EntryId.HasValue)
            {
                entry = snapshot.Entries.FirstOrDefault(a => a.Id == request.EntryId);
                if (entry == null)
                    return Unprocessable($"Cannot find entry with id={request.EntryId}");

                entry.SetCategory(category);
                entry.SetSubject(subject);
                entry.SetQuantity(request.Quantity);
                entry.SetPrice(request.Price);

                entry.Description = request.Description;
                entry.Type = request.Type;
            }
            else
            {
                entry = new Db.Entities.Entry(snapshot, request.Type, subject, category, request.Quantity, request.Price)
                {
                    Description = request.Description
                };
                await _db.Entries.AddAsync(entry, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            return entry.Id;
        }
    }
}
