using Glimpse.Operations.Profiles;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Net;

namespace Glimpse.Tests.Operations
{
    public class SaveProfileTests : IntegrationTestsBase
    {
        public SaveProfileTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {

        }

        [Fact]
        public async Task Should_create_profile()
        {
            var command = new SaveProfile.Command
            {
                Name = AutoFx.Create<string>(),
                Description = AutoFx.Create<string>()
            };
            var url = "/Profiles/Save";
            var id = await PostShouldSucceed(url, command);

            var created = await ExecuteDbContext(a => a.Profiles.SingleOrDefaultAsync(a => a.Id == id));

            created.ShouldNotBeNull();
            created.Name.ShouldBe(command.Name);
            created.Description.ShouldBe(command.Description);
        }

        [Fact]
        public async Task Should_not_create_profile_without_name()
        {
            var command = new SaveProfile.Command
            { 
                Name = "",
                Description = AutoFx.Create<string>()
            };
            var url = "/Profiles/Save";
            var errors = await PostShouldFail(url, command, HttpStatusCode.BadRequest);

            errors.ShouldNotBeNull();
            errors.Length.ShouldBe(1);
            errors[0].ShouldBe("The Name field is required.");
        }

        [Fact]
        public async Task Should_update_profile()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>())
            {
                Description = AutoFx.Create<string>()
            };
            await Insert(profile);

            var command = new SaveProfile.Command
            {
                ProfileId = profile.Id,
                Name = AutoFx.Create<string>(),
                Description = AutoFx.Create<string>()
            };
            var url = "/Profiles/Save";
            var id = await PostShouldSucceed(url, command);

            var created = await ExecuteDbContext(a => a.Profiles.SingleOrDefaultAsync(a => a.Id == id));

            created.ShouldNotBeNull();
            created.Id.ShouldBe(profile.Id);
            created.Name.ShouldBe(command.Name);
            created.Description.ShouldBe(command.Description);
        }

        //[Fact]
        //public async Task Should_not_create_profile_without_name()
        //{
        //    var url = $"/Profiles/Get/0";
        //    var errors = await GetShouldFail(url);

        //    errors.ShouldNotBeNull();
        //    errors.Length.ShouldBe(1);
        //    errors[0].ShouldContain("Cannot find profile");
        //}
    }
}
