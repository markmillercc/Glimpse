using Glimpse.Db;
using Glimpse.Operations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using System.Net;
using System.Text;

namespace Glimpse.Tests;
public abstract class IntegrationTestsBase
    : IClassFixture<IntegrationTestsWebApplicationFactory<Program>>
{
    protected readonly IntegrationTestsWebApplicationFactory<Program> _factory;
    public IntegrationTestsBase(IntegrationTestsWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task ExecuteDbContext(Func<GlimpseDbContext, Task> action) =>
        await ExecuteScopeAsync(p => action(p.GetRequiredService<GlimpseDbContext>()));

    public async Task<T> ExecuteDbContext<T>(Func<GlimpseDbContext, Task<T>> action) =>
        await ExecuteScopeAsync(p => action(p.GetRequiredService<GlimpseDbContext>()));

    public async Task Insert(params object[] entities)
    {
        await ExecuteDbContext(db =>
        {
            db.AddRangeAsync(entities);

            return db.SaveChangesAsync();
        });
    }

    public async Task<T> GetShouldSucceed<T>(string url)
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync(url);

        return await ShouldBeOkStatusCode<T>(response);
    }

    public async Task<T> DeleteShouldSucceed<T>(string url)
    {
        using var client = _factory.CreateClient();
        var response = await client.DeleteAsync(url);

        return await ShouldBeOkStatusCode<T>(response);
    }

    public async Task<T> PostShouldSucceed<T>(string url, Operation.IRequest<T> request)
    {
        var body = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        using var client = _factory.CreateClient();
        var response = await client.PostAsync(url, body);

        return await ShouldBeOkStatusCode<T>(response);
    }

    public async Task<string[]> PostShouldFail<T>(string url, Operation.IRequest<T> request, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
    {
        var body = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        using var client = _factory.CreateClient();
        var response = await client.PostAsync(url, body);

        return await ShouldBeBadRequestStatusCode(response, expectedStatusCode);

    }

    public async Task<string[]> GetShouldFail(string url, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync(url);

        return await ShouldBeBadRequestStatusCode(response, expectedStatusCode);
    }

    private static async Task<string[]> ShouldBeBadRequestStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
    {
        response.IsSuccessStatusCode.ShouldBeFalse(response.StatusCode.ToString());

        if (response.StatusCode != expectedStatusCode)
        {
            Assert.Fail($"Response should have had status code {expectedStatusCode} ({(int)expectedStatusCode}) but did not\n{DescribeResponse(response)}");
        }

        response.Content.ShouldNotBeNull();

        var content = await response.Content.ReadAsStringAsync();

        var errors = JsonConvert.DeserializeAnonymousType(content, new { Errors = Array.Empty<string>() })
            ?.Errors;

        return errors;
    }

    private static async Task<T> ShouldBeOkStatusCode<T>(HttpResponseMessage responseMessage)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();
            try
            {
                var result = JsonConvert.DeserializeObject<T>(content);
                return (T)result;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Status code was Ok but could not deserialize content to type '{typeof(T)}'\n{ex}");
            }
        }

        Assert.Fail($"Response should have had Ok status code but did not\n{DescribeResponse(responseMessage)}");

        return default;
    }

    private static string DescribeResponse(HttpResponseMessage responseMessage)
    {
        var requestMethod = responseMessage.RequestMessage?.Method?.Method ?? "Unknown";
        var requestUrl = responseMessage.RequestMessage?.RequestUri?.AbsolutePath ?? "Unknown";

        var response = "Response was null";
        if (responseMessage != null)
        {
            var statusCodeName = responseMessage.StatusCode.ToString();
            var statusCode = (int)responseMessage.StatusCode;
            var content = responseMessage.Content.ReadAsStringAsync().Result;

            if (string.IsNullOrEmpty(content))
            {
                content = "No content";
            }
            else
            {
                content = JToken.Parse(content)
                  .ToString(Formatting.Indented)
                  .Replace("\\r\\n", "\n").Replace("\\t", "\t");
            }

            response = $"Response: {statusCodeName} ({statusCode})\n{content}";
        }
        return $"{requestMethod} {requestUrl}\n{response}";
    }

    private async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GlimpseDbContext>();

        try
        {
            await db.BeginTransactionAsync(default).ConfigureAwait(false);
            await action(scope.ServiceProvider).ConfigureAwait(false);
            await db.CommitTransactionAsync(default).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Test exception: {e}");
            await db.RollbackTransactionAsync(default);
            throw;
        }
    }

    private async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GlimpseDbContext>();

        try
        {
            await db.BeginTransactionAsync(default).ConfigureAwait(false);
            var result = await action(scope.ServiceProvider).ConfigureAwait(false);
            await db.CommitTransactionAsync(default).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await db.RollbackTransactionAsync(default);
            throw;
        }
    }
}
