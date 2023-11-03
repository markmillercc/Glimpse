using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Glimpse.Tests.Operations;

public class DeleteEntryTests : IntegrationTestsBase
{
    public DeleteEntryTests(IntegrationTestsWebApplicationFactory<Program> factory)
        : base(factory)
    {

    }

    [Fact]
    public async Task Should_delete_entry()
    {
        var profile = new Db.Entities.Profile(AutoFx.Create<string>());
        var subject = new Db.Entities.Subject(profile, AutoFx.Create<string>());
        var category = new Db.Entities.Category(profile, AutoFx.Create<string>());
        var snapshot = new Db.Entities.Snapshot(profile, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(10));
        var entry = new Db.Entities.Entry(snapshot, Db.Entities.EntryType.BalanceSheet, subject, category, 1, 5);

        await Insert(profile, subject, category, snapshot, entry);

        var url = $"snapshots/entries/delete/{entry.Id}";
        var worked = await DeleteShouldSucceed<bool>(url);

        worked.ShouldBeTrue();

        var exists = await ExecuteDbContext(db => db.Entries.AnyAsync(a => a.Id == entry.Id));

        exists.ShouldBeFalse();
    }
}

