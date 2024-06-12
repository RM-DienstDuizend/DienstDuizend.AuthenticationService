using DienstDuizend.AuthenticationService.Common.Interfaces;
using DienstDuizend.AuthenticationService.Infrastructure.Exceptions.Handlers;
using DienstDuizend.AuthenticationService.Infrastructure.Persistence;
using DienstDuizend.AuthenticationService.Infrastructure.Services;
using FluentValidation;
using Google.Authenticator;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;

namespace DienstDuizend.AuthenticationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHandlers();
        services.AddBehaviors();

        services.AddExceptionHandler<ApplicationExceptionHandler>();
        services.AddExceptionHandler<FluentValidationExceptionHandler>();
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DbConnection");
            options.UseNpgsql(connectionString);
        });

        services.AddOpenTelemetry()
            .WithMetrics(builder => builder
                // Metrics provider from OpenTelemetry
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddAspNetCoreInstrumentation()
                // Metrics provides by ASP.NET Core in .NET 8
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddPrometheusExporter()); // We use v1.7 because currently v1.8 has an issue with formatting.


        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();
            busConfigurator.AddConsumers(typeof(IAssemblyMarker).Assembly);
            
            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(configuration.GetConnectionString("MessageBroker"));
                
                configurator.ConfigureEndpoints(context);
            });
        });

        services.Configure<JwtAuthSettings>(configuration.GetSection("JwtAuthSettings"));
        services.AddSingleton<IJwtAuthTokenService, JwtAuthTokenService>();
        
        
        // serviceCollection.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>();

        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        services.AddSingleton<TwoFactorAuthenticator>();
        
        return services;
    }
}