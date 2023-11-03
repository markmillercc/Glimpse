using Shouldly;
using Glimpse.Operations.Snapshots;

namespace Glimpse.Tests.Operations
{
    public class GetSnapshotTests : IntegrationTestsBase
    {
        public GetSnapshotTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Should_get_simple_snapshot()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var snapshot = new Db.Entities.Snapshot(profile, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(10));

            await Insert(profile, snapshot);

            var url = $"Snapshots/Get/{snapshot.Id}";
            var dto = await GetShouldSucceed<SnapshotModel>(url);

            dto.ShouldNotBeNull();
            dto.Start.ShouldBe(snapshot.Start);
            dto.End.ShouldBe(snapshot.End);
            dto.CashFlowNet.ShouldBe(0m);
            dto.NetWorth.ShouldBe(0m);
        }
    }
}
