using Glimpse.Db;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Glimpse.Tests;

public class IntegrationTestsWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private static bool _isDbInitialized = false;
    private static readonly object _locker = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Remove(
                services.Single(a => a.ServiceType == typeof(DbContextOptions<GlimpseDbContext>)));

            services.Remove(
                services.Single(a => a.ServiceType == typeof(GlimpseDbContext)));

            services.AddDbContext<GlimpseDbContext>(options =>
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GlimpseTests2"));

            lock (_locker)
            {
                if (!_isDbInitialized)
                {
                    using var provider = services.BuildServiceProvider();
                    using var db = provider.GetRequiredService<GlimpseDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.Migrate();
                    _isDbInitialized = true;
                }
            }
            
        });
    }
}
