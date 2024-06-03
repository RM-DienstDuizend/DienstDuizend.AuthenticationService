using DienstDuizend.AuthenticationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DienstDuizend.AuthService.IntegrationTesting.Setup;

[Collection(nameof(IntegrationTestCollection))]
public abstract class IntegrationTest : IAsyncLifetime
{
    public readonly WebAppFactory WebAppFactory;
    public readonly IServiceScope Scope;
    public readonly ApplicationDbContext Db;
    public readonly HttpClient HttpClient;

    public IntegrationTest(WebAppFactory webAppFactory)
    {
        WebAppFactory = webAppFactory;
        Scope = webAppFactory.Services.CreateScope();
        HttpClient = webAppFactory.CreateClient();

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseNpgsql(WebAppFactory.DbConnectionString);
        Db = new ApplicationDbContext(builder.Options);
        
        //Disable tracking enables reads on updated entities ( tests)
        Db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await WebAppFactory.ResetDatabaseAsync();
        Scope.Dispose();
    }
}