using Glimpse.Operations.Subjects;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Net;

namespace Glimpse.Tests.Operations
{

    public class SaveSubjectTests : IntegrationTestsBase
    {
        public SaveSubjectTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Should_create_subject()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            await Insert(profile);

            var command = new SaveSubject.Command
            {
                ProfileId = profile.Id,
                Name = AutoFx.Create<string>(),
                Description = AutoFx.Create<string>()
            };
            var url = "subjects/save";
            var response = await PostShouldSucceed(url, command);

            response.ShouldBeGreaterThan(0);

            var created = await ExecuteDbContext(db => db.Subjects
                .Include(a => a.Profile)
                .SingleOrDefaultAsync(a => a.Id == response));

            created.ShouldNotBeNull();
            created.Name.ShouldBe(command.Name);
            created.Description.ShouldBe(command.Description);
            created.ProfileId.ShouldBe(command.ProfileId);
            created.Profile.Name.ShouldBe(profile.Name);
        }

        [Fact]
        public async Task Should_create_second_subject()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var subject = new Db.Entities.Subject(profile, AutoFx.Create<string>());
            await Insert(profile, subject);

            var command = new SaveSubject.Command
            {
                ProfileId = profile.Id,
                Name = AutoFx.Create<string>()
            };
            var url = "subjects/save";
            var id = await PostShouldSucceed(url, command);

            id.ShouldBeGreaterThan(0);

            var created = await ExecuteDbContext(db => db.Subjects
                .Include(a => a.Profile)
                .ThenInclude(a => a.Subjects)
                .SingleOrDefaultAsync(a => a.Id == id));

            created.ShouldNotBeNull();
            created.Name.ShouldBe(command.Name);
            created.ProfileId.ShouldBe(profile.Id);
            created.Profile.Name.ShouldBe(profile.Name);
            created.Profile.Subjects.Count().ShouldBe(2);
        }

        [Fact]
        public async Task Should_not_create_subject_without_profile()
        {
            var command = new SaveSubject.Command
            {
                ProfileId = 0,
                Name = AutoFx.Create<string>(),
                Description = AutoFx.Create<string>()
            };
            var url = "subjects/save";
            var errors = await PostShouldFail(url, command, HttpStatusCode.UnprocessableEntity);

            errors.ShouldNotBeNull();
            errors.ShouldNotBeEmpty();

            errors.ShouldContain("Cannot find profile id=0");
        }

        [Fact]
        public async Task Should_not_create_duplicate_subject()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var subject = new Db.Entities.Subject(profile, AutoFx.Create<string>());
            await Insert(profile, subject);

            var command = new SaveSubject.Command
            {
                ProfileId = profile.Id,
                Name = subject.Name
            };
            var url = "subjects/save";
            var errors = await PostShouldFail(url, command, HttpStatusCode.UnprocessableEntity);

            errors.ShouldNotBeNull();
            errors.Length.ShouldBe(1);
            errors[0].ShouldContain($"already has a subject named {subject.Name}");

            var count = await ExecuteDbContext(db => db.Subjects
                .CountAsync(a => a.ProfileId == command.ProfileId && a.Name == command.Name));

            count.ShouldBe(1);
        }
    }
}
