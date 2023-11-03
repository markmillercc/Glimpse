using Glimpse.Operations.Categories;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Net;

namespace Glimpse.Tests.Operations
{
    public class SaveCategoryTests : IntegrationTestsBase
    {
        public SaveCategoryTests(IntegrationTestsWebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Should_create_category()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            await Insert(profile);

            var command = new SaveCategory.Command
            {                
                ProfileId = profile.Id,
                Name = AutoFx.Create<string>()
            };
            var url = "categories/save";
            var response = await PostShouldSucceed(url, command);

            response.ShouldBeGreaterThan(0);

            var created = await ExecuteDbContext(db => db.Categories
                .Include(a => a.Profile)
                .SingleOrDefaultAsync(a => a.ProfileId == command.ProfileId && a.Name == command.Name));

            created.ShouldNotBeNull();
            created.Name.ShouldBe(command.Name);
            created.ProfileId.ShouldBe(profile.Id);
            created.Profile.Name.ShouldBe(profile.Name);
        }

        [Fact]
        public async Task Should_create_second_category()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var category = new Db.Entities.Category(profile, AutoFx.Create<string>());
            await Insert(profile, category);

            var command = new SaveCategory.Command
            {
                ProfileId = profile.Id,
                Name = AutoFx.Create<string>()
            };
            var url = "categories/save";
            var id = await PostShouldSucceed(url, command);

            id.ShouldBeGreaterThan(0);

            var created = await ExecuteDbContext(db => db.Categories
                .Include(a => a.Profile)
                .ThenInclude(a => a.Categories)
                .SingleOrDefaultAsync(a => a.Id == id));

            created.ShouldNotBeNull();
            created.Name.ShouldBe(command.Name);
            created.ProfileId.ShouldBe(profile.Id);
            created.Profile.Name.ShouldBe(profile.Name);
            created.Profile.Categories.Count().ShouldBe(2);
        }

        [Theory]
        [InlineData(0, "Cannot find profile")]
        [InlineData(9999999, "Cannot find profile")]
        public async Task Should_not_create_category_without_profile(int badProfileId, string expectedError)
        {            
            var command = new SaveCategory.Command
            {
                ProfileId = badProfileId,
                Name = AutoFx.Create<string>()
            };
            var url = "categories/save";
            var errors = await PostShouldFail(url, command, HttpStatusCode.UnprocessableEntity);

            errors.ShouldNotBeNull();
            errors.ShouldNotBeEmpty();
            errors.Length.ShouldBe(1);

            errors[0].ShouldContain(expectedError);
        }

        [Fact]
        public async Task Should_not_create_duplicate_category()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var category = new Db.Entities.Category(profile, AutoFx.Create<string>());
            await Insert(profile, category);

            var command = new SaveCategory.Command
            {
                ProfileId = profile.Id,
                Name = category.Name
            };
            var url = "categories/save";
            var errors = await PostShouldFail(url, command, HttpStatusCode.UnprocessableEntity);

            errors.ShouldNotBeNull();
            errors.Length.ShouldBe(1);
            errors[0].ShouldContain($"already has a category named {category.Name}");

            var count = await ExecuteDbContext(db => db.Categories
                .CountAsync(a => a.ProfileId == command.ProfileId && a.Name == command.Name));

            count.ShouldBe(1);
        }

        [Fact]
        public async Task Should_update_category()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var category = new Db.Entities.Category(profile, AutoFx.Create<string>());
            await Insert(profile, category);

            var command = new SaveCategory.Command
            {
                CategoryId = category.Id,
                ProfileId = profile.Id,
                Name = AutoFx.Create<string>()
            };

            var url = "categories/save";
            var id = await PostShouldSucceed(url, command);

            id.ShouldBe(category.Id);

            var created = await ExecuteDbContext(db => db.Categories
                .Include(a => a.Profile)
                .SingleOrDefaultAsync(a => a.Id == id));

            created.ShouldNotBeNull();
            created.Name.ShouldBe(command.Name);
            created.ProfileId.ShouldBe(profile.Id);
            created.Profile.Name.ShouldBe(profile.Name);
        }


        [Fact]
        public async Task Should_not_update_to_duplicate_category()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());
            var category1 = new Db.Entities.Category(profile, AutoFx.Create<string>());
            var category2 = new Db.Entities.Category(profile, AutoFx.Create<string>());
            await Insert(profile, category1, category2);

            var command = new SaveCategory.Command
            {
                CategoryId = category1.Id,
                ProfileId = profile.Id,
                Name = category2.Name
            };

            var url = "categories/save";
            var errors = await PostShouldFail(url, command, HttpStatusCode.UnprocessableEntity);

            errors.ShouldNotBeNull();
            errors.Length.ShouldBe(1);
            errors[0].ShouldContain($"already has a category named {category2.Name}");

            var cats = await ExecuteDbContext(db => db.Categories
                .Where(a => a.ProfileId == profile.Id).ToListAsync());

            cats.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Should_not_allow_category_id_zero()
        {
            var profile = new Db.Entities.Profile(AutoFx.Create<string>());            
            await Insert(profile);

            var command = new SaveCategory.Command
            {
                CategoryId = 0,
                ProfileId = profile.Id,
                Name = AutoFx.Create<string>()
            };

            var url = "categories/save";
            var errors = await PostShouldFail(url, command, HttpStatusCode.UnprocessableEntity);

            errors.ShouldNotBeNull();
            errors.Length.ShouldBe(1);
            errors[0].ShouldContain("Unable to find category");

            var cats = await ExecuteDbContext(db => db.Categories
                .Where(a => a.ProfileId == profile.Id).ToListAsync());

            cats.Count.ShouldBe(0);
        }
    }
}
