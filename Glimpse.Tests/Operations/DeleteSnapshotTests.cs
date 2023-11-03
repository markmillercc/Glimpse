using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Glimpse.Tests.Operations
{
    public class DeleteSnapshotTests : IntegrationTestsBase
    {
        public DeleteSnapshotTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task Should_delete_snapshot()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var snapshot = new Db.Entities.Snapshot(profile, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(10));

            await Insert(profile, snapshot);

            var url = $"Snapshots/Delete/{snapshot.Id}";
            var worked = await DeleteShouldSucceed<bool>(url);

            worked.ShouldBeTrue();

            var exists = await ExecuteDbContext(db => db.Snapshots.AnyAsync(a => a.Id == snapshot.Id));

            exists.ShouldBeFalse();
        }
    }
}
