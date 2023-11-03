using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Glimpse.Tests.Operations
{
    public class DeleteProfileTests : IntegrationTestsBase
    {
        public DeleteProfileTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task Should_delete_profile()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());            
            await Insert(profile);

            var url = $"profiles/delete/{profile.Id}";
            var worked = await DeleteShouldSucceed<bool>(url);

            worked.ShouldBeTrue();

            var exists = await ExecuteDbContext(db => db.Profiles.AnyAsync(a => a.Id == profile.Id));

            exists.ShouldBeFalse();
        }
    }
}
