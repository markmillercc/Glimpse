using Glimpse.Operations.Snapshots;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Net;

namespace Glimpse.Tests.Operations
{
    public class SaveSnapshotTests : IntegrationTestsBase
    {
        public SaveSnapshotTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Should_create_snapshot()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            await Insert(profile);

            var command = new SaveSnapshot.Command
            {
                ProfileId = profile.Id,
                Start = DateTime.UtcNow.AddDays(-10),
                End = DateTime.UtcNow.AddDays(10)
            };
            var url = "snapshots/save";
            var id = await PostShouldSucceed(url, command);

            id.ShouldBeGreaterThan(0);

            var created = await ExecuteDbContext(db => db.Snapshots
                .Include(a => a.Profile)
                .Include(a => a.Entries)
                .SingleOrDefaultAsync(a => a.Id == id));

            created.ShouldNotBeNull();
            created.Start.ShouldBe(command.Start.Value);
            created.End.ShouldBe(command.End.Value);
            created.Entries.ShouldBeEmpty();
            created.ProfileId.ShouldBe(profile.Id);
            created.Profile.Name.ShouldBe(profile.Name);
        }

        [Fact]
        public async Task Should_create_second_snapshot()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var snapshot = new Db.Entities.Snapshot(profile, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(10));
            await Insert(profile, snapshot);

            var command = new SaveSnapshot.Command
            {
                ProfileId = profile.Id,
                Start = DateTime.UtcNow.AddDays(20),
                End = DateTime.UtcNow.AddDays(40)
            };
            var url = "snapshots/save";
            var id = await PostShouldSucceed(url, command);

            id.ShouldBeGreaterThan(0);

            var created = await ExecuteDbContext(db => db.Snapshots
                .Include(a => a.Profile)
                    .ThenInclude(a => a.Snapshots)
                .Include(a => a.Entries)
                .SingleOrDefaultAsync(a => a.Id == id));

            created.ShouldNotBeNull();
            created.Start.ShouldBe(command.Start.Value);
            created.End.ShouldBe(command.End.Value);
            created.Entries.ShouldBeEmpty();
            created.ProfileId.ShouldBe(profile.Id);
            created.Profile.Name.ShouldBe(profile.Name);
            created.Profile.Snapshots.Count().ShouldBe(2);
        }

        [Theory]
        [InlineData(10, 12, "End date cannot be before start date", HttpStatusCode.UnprocessableEntity)]
        [InlineData(null, 10, "The Start field is required.", HttpStatusCode.BadRequest)]
        [InlineData(10, null, "The End field is required.", HttpStatusCode.BadRequest)]
        public async Task Should_not_create_snapshot_with_invalid_dates(int? startsDaysAgo, int? endsDaysAgo, string expectedError, HttpStatusCode expectedStatusCode)
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            await Insert(profile);

            var command = new SaveSnapshot.Command
            {
                ProfileId = profile.Id
            };

            if (startsDaysAgo.HasValue)
                command.Start = DateTime.UtcNow.AddDays(-startsDaysAgo.Value);

            if (endsDaysAgo.HasValue)
                command.End = DateTime.UtcNow.AddDays(-endsDaysAgo.Value);

            var url = "snapshots/save";
            var errors = await PostShouldFail(url, command, expectedStatusCode);

            errors.ShouldNotBeNull();
            errors.Length.ShouldBe(1);
            errors[0].ShouldBe(expectedError);

            var created = await ExecuteDbContext(db => db.Snapshots.CountAsync(a => a.ProfileId == profile.Id));
            created.ShouldBe(0);
        }

        [Theory]
        [InlineData(0, "Cannot find profile")]
        [InlineData(999999, "Cannot find profile")]
        public async Task Should_not_create_snapshot_with_invalid_profile(int badProfileId, string expectedError)
        {
            var command = new SaveSnapshot.Command
            {
                ProfileId = badProfileId,
                Start = DateTime.UtcNow.AddDays(-10),
                End = DateTime.UtcNow.AddDays(10)
            };

            var url = "snapshots/save";
            var errors = await PostShouldFail(url, command, HttpStatusCode.UnprocessableEntity);

            errors.ShouldNotBeNull();
            errors.Length.ShouldBe(1);
            errors[0].ShouldContain(expectedError);
        }
    }
}
