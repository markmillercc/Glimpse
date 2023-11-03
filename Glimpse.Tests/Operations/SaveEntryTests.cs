using Glimpse.Operations.Snapshots;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Glimpse.Tests.Operations
{
    public class SaveEntryTests : IntegrationTestsBase
    {
        public SaveEntryTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task Should_create_entry()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var subject = new Db.Entities.Subject(profile, AutoFx.Create<string>());
            var category = new Db.Entities.Category(profile, AutoFx.Create<string>());
            var snapshot = new Db.Entities.Snapshot(profile, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(10));            

            await Insert(profile, subject, category, snapshot);

            var command = new SaveEntry.Command
            {
                Type = Db.Entities.EntryType.CashFlow,
                SnapshotId = snapshot.Id,
                SubjectId = subject.Id,
                CategoryId = category.Id,
                Quantity = 1,
                Price = 5,
                Description = AutoFx.Create<string>()
            };
            var url = "snapshots/entries/save";
            var id = await PostShouldSucceed(url, command);

            id.ShouldBeGreaterThan(0);

            var created = await ExecuteDbContext(db => db.Entries.FirstOrDefaultAsync(a => a.Id == id));

            created.ShouldNotBeNull();
            created.SnapshotId.ShouldBe(command.SnapshotId);
            created.SubjectId.ShouldBe(command.SubjectId);
            created.CategoryId.ShouldBe(command.CategoryId);
            created.Quantity.ShouldBe(command.Quantity);
            created.Price.ShouldBe(command.Price);
            created.Description.ShouldBe(command.Description);            
        }

        //[Fact]
        //public async Task Should_update_entry()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
