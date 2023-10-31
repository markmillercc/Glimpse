using Glimpse.Db;
using MediatR.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Glimpse.Operations
{
    [Endpoint(Method.Delete)]
    public class GetCategoryList
    {
        public class Query : Operation.IRequest<IEnumerable<CategoryModel>>
        {
            public int? ProfileId { get; set; }
            public int? SnapshotId { get; set; }
            public int? CategoryId { get; set; }
        }

        public class CategoryModel
        {
            public int CategoryId { get; set; }
            public int ProfileId { get; set; }
            public string Name { get; set; }
        }

        public class QueryHandler : Operation.Handler<Query, IEnumerable<CategoryModel>>
        {
            private readonly GlimpseDbContext _db;

            public QueryHandler(GlimpseDbContext db)
            {
                _db = db;
            }

            protected override async Task<IEnumerable<CategoryModel>> DoOperation(Query request, CancellationToken cancellationToken)
            {
                IQueryable<Db.Entities.Category> query;
                if (request.CategoryId.HasValue)
                {
                    query = _db.Categories
                        .Where(a => a.Id == request.CategoryId.Value);
                }
                else if (request.SnapshotId.HasValue)
                {
                    query = _db.Entries
                        .Where(e => e.SnapshotId == request.SnapshotId.Value)
                        .Select(e => e.Category)
                        .Distinct();
                }
                else if (request.ProfileId.HasValue)
                {
                    query = _db.Categories
                        .Where(a => a.ProfileId == request.ProfileId);
                }
                else
                {
                    return Error("Category id, snapshot id, or profile id must be provided");
                }

                var categories = await query.Select(category => new CategoryModel
                {
                    CategoryId = category.Id,
                    ProfileId = category.ProfileId,
                    Name = category.Name
                }).ToListAsync(cancellationToken);

                return categories;
            }
        }
    }
}
