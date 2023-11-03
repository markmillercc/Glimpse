using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Glimpse.Tests.Operations
{
    public class DeleteSubjectTests : IntegrationTestsBase
    {
        public DeleteSubjectTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task Should_delete_subject()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var subject = new Db.Entities.Subject(profile, AutoFx.Create<string>());
            await Insert(profile, subject);

            var url = $"subjects/delete/{subject.Id}";
            var worked = await DeleteShouldSucceed<bool>(url);

            worked.ShouldBeTrue();

            var exists = await ExecuteDbContext(db => db.Subjects.AnyAsync(a => a.Id == subject.Id));

            exists.ShouldBeFalse();
        }
    }
}
