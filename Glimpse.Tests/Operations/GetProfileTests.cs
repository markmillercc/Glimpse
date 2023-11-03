using Glimpse.Operations.Profiles;
using Shouldly;
using System.Net;

namespace Glimpse.Tests.Operations
{
    public class GetProfileTests : IntegrationTestsBase
    {
        public GetProfileTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task Should_get_profile()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());

            var subject1 = new Db.Entities.Subject(profile, AutoFx.Create<string>());
            var subject2 = new Db.Entities.Subject(profile, AutoFx.Create<string>());
            var subject3 = new Db.Entities.Subject(profile, AutoFx.Create<string>());

            var snapshot1 = new Db.Entities.Snapshot(profile, new DateTime(2020, 1, 1), new DateTime(2020, 2, 1));
            var snapshot2 = new Db.Entities.Snapshot(profile, new DateTime(2020, 2, 1), new DateTime(2020, 3, 1));
            var snapshot3 = new Db.Entities.Snapshot(profile, new DateTime(2020, 3, 1), new DateTime(2020, 4, 1));

            var category1 = new Db.Entities.Category(profile, AutoFx.Create<string>());
            var category2 = new Db.Entities.Category(profile, AutoFx.Create<string>());
            var category3 = new Db.Entities.Category(profile, AutoFx.Create<string>());

            await Insert(profile, subject1, subject2, subject3,
                snapshot1, snapshot2, snapshot3, category1, category2, category3);

            var url = $"/Profiles/Get/{profile.Id}";
            var details = await GetShouldSucceed<GetProfile.ProfileModel>(url);

            details.ShouldNotBeNull();
            details.Name.ShouldBe(profile.Name);
            details.Description.ShouldBe(profile.Description);
            details.Subjects.Count().ShouldBe(3);
            details.Snapshots.Count().ShouldBe(3);
            details.Categories.Count().ShouldBe(3);
            details.Snapshots.ShouldSatisfyAllConditions(
                a => a.Any(s => s.Start == snapshot1.Start && s.End == snapshot1.End),
                a => a.Any(s => s.Start == snapshot2.Start && s.End == snapshot2.End),
                a => a.Any(s => s.Start == snapshot3.Start && s.End == snapshot3.End));

            details.Subjects.ShouldSatisfyAllConditions(
                a => a.Any(s => s.Key == subject1.Id && s.Value == subject1.Name),
                a => a.Any(s => s.Key == subject2.Id && s.Value == subject2.Name),
                a => a.Any(s => s.Key == subject3.Id && s.Value == subject3.Name));

            details.Categories.ShouldSatisfyAllConditions(
                a => a.Any(s => s.Key == category1.Id && s.Value == category1.Name),
                a => a.Any(s => s.Key == category2.Id && s.Value == category2.Name),
                a => a.Any(s => s.Key == category3.Id && s.Value == category3.Name));
        }

        [Fact]
        public async Task Should_return_error_when_profile_does_not_exist()
        {
            var url = $"/Profiles/Get/0";
            var errors = await GetShouldFail(url, HttpStatusCode.UnprocessableEntity);

            errors.ShouldNotBeNull();
            errors.Length.ShouldBe(1);
            errors[0].ShouldContain("Cannot find profile");            
        }
    }
}
