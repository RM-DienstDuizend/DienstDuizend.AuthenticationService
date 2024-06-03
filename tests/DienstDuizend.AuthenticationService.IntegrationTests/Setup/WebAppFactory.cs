using System.Data.Common;
using DienstDuizend.AuthenticationService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace DienstDuizend.AuthService.IntegrationTesting.Setup
{
    public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public string DbConnectionString { get; private set; } = string.Empty;

        private struct DatabaseAccess
        {
            public const string Username = "fake_user";
            public const string Password = "AuthService";
            public const string DatabaseName = "integration_tests";
        }

        private readonly PostgreSqlContainer _dbContainer;

        private Respawner respawner = default!;
        private DatabaseFacade _databaseFacade = default!;

        public WebAppFactory()
        {
            _dbContainer = new PostgreSqlBuilder()
                .WithDatabase(DatabaseAccess.DatabaseName)
                .WithUsername(DatabaseAccess.Username)
                .WithPassword(DatabaseAccess.Password)
                .Build();
            
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "JwtAuthSettings:AccessTokenSecret", "ACCESS_TOKEN_SECRET" },
                    { "JwtAuthSettings:AccessTokenExpirationTimeInMinutes", "5" },
                    { "JwtAuthSettings:RefreshTokenSecret", "REFRESH_TOKEN_SECRET" },
                    { "JwtAuthSettings:RefreshTokenExpirationTimeInMinutes", "15" },
                    { "LockoutSettings:LockoutKeyLength", "32" },
                    { "LockoutSettings:MaxAttempts", "3" }
                }!);
                    
            });
            
            
            builder.ConfigureTestServices(services =>
            {
                services.AddMassTransitTestHarness();
                
                services.RemoveByType<DbContextOptions<ApplicationDbContext>>();

                services.AddDbContext<ApplicationDbContext>(options => { options.UseNpgsql(DbConnectionString); });

                var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                _databaseFacade = context.Database;

                context.Database.Migrate();
                InitRespawner().Wait();
            });
        }

        public async Task InitRespawner()
        {
            DbConnection conn = await GetOpenedDbConnectionAsync();
            respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                SchemasToInclude = new[] { "public" },
                TablesToIgnore = new Table[]
                {
                    "__EFMigrationsHistory",
                },
                DbAdapter = DbAdapter.Postgres
            });
        }

        public async Task ResetDatabaseAsync()
        {
            DbConnection conn = await GetOpenedDbConnectionAsync();
            await respawner.ResetAsync(conn);
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync().ConfigureAwait(false);
            DbConnectionString = _dbContainer.GetConnectionString();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();
        }

        private async Task<DbConnection> GetOpenedDbConnectionAsync()
        {
            var conn = _databaseFacade.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();
            return conn;
        }
        
    }
}