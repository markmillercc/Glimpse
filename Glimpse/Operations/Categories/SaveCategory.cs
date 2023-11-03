using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace Glimpse.Operations.Categories;

[Endpoint(Method.Post, "save", Group = "categories")]
public class SaveCategory
{
    public class Command : Operation.IRequest<int>
    { 
        public int? CategoryId { get; set; }
        public int ProfileId { get; set; }
        [Required]            
        public string Name { get; set; }
    }

    public class CommandHandler : Operation.Handler<Command, int>
    {
        private readonly GlimpseDbContext _db;

        public CommandHandler(GlimpseDbContext db) => _db = db;

        protected override async Task<int> DoOperation(Command request, CancellationToken cancellationToken)
        {
            var profile = await _db.Profiles
                .Include(a => a.Categories)
                .FirstOrDefaultAsync(a => a.Id == request.ProfileId, cancellationToken);

            if (profile == null)
                return Unprocessable($"Cannot find profile with Id={request.ProfileId}");

            var categories = profile.Categories.Where(a => request.CategoryId == null || a.Id != request.CategoryId.Value).ToList();
            if (categories.Any(a => a.Name.ToLower() == request.Name.ToLower()))
                return Unprocessable($"Profile Id={profile.Id} already has a category named {request.Name}");

            Db.Entities.Category category;
            if (request.CategoryId.HasValue)
            {
                category = profile.Categories.FirstOrDefault(a => a.Id == request.CategoryId.Value);
                if (category == null)
                    return Unprocessable($"Unable to find category with id={request.CategoryId.Value} and profile id={profile.Id}");

                category.Name = request.Name;
            }
            else
            {
                category = new Db.Entities.Category(profile, request.Name);
                await _db.Categories.AddAsync(category, cancellationToken);
            }
            
            await _db.SaveChangesAsync(cancellationToken);
            return category.Id;
        }            
    }
}
