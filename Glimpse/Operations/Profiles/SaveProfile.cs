using Glimpse.Db;
using MediatR.Endpoints;
using System.ComponentModel.DataAnnotations;

namespace Glimpse.Operations.Profiles;

[Endpoint(Method.Post, "save", Group = "profiles")]
public class SaveProfile
{
    public class Command : Operation.IRequest<int>
    {
        public int? ProfileId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class CommandHandler : Operation.Handler<Command, int>
    {
        private readonly GlimpseDbContext _db;
        public CommandHandler(GlimpseDbContext db) => _db = db;

        protected override async Task<int> DoOperation(Command request, CancellationToken cancellationToken)
        {
            //if (string.IsNullOrWhiteSpace(request.Name))
            //    return Error("Name is required");

            Db.Entities.Profile profile;
            if (request.ProfileId.HasValue)
            {
                profile = await _db.Profiles.FindAsync(request.ProfileId.Value, cancellationToken);
                profile.Name = request.Name;
                profile.Description = request.Description;
            }
            else
            {
                profile = new Db.Entities.Profile(request.Name)
                {
                    Description = request.Description
                };
                await _db.Profiles.AddAsync(profile, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            return profile.Id;
        }
    }
}
