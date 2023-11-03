using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Glimpse.Tests.Operations
{
    public class DeleteCategoryTests : IntegrationTestsBase
    {
        public DeleteCategoryTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task Should_delete_category()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var category = new Db.Entities.Category(profile, AutoFx.Create<string>());
            await Insert(profile, category);

            var url = $"categories/delete/{category.Id}";
            var worked = await DeleteShouldSucceed<bool>(url);

            worked.ShouldBeTrue();

            var exists = await ExecuteDbContext(db => db.Categories.AnyAsync(a => a.Id == category.Id));

            exists.ShouldBeFalse();
        }
    }
}
