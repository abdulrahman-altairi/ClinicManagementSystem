using ClinicManagementSystem.Application.Common.Interfaces;
using ClinicManagementSystem.Application.Interfaces.Repositories;
using ClinicManagementSystem.Infrastructure.ExternalServices;
using ClinicManagementSystem.Infrastructure.Identity;
using ClinicManagementSystem.Infrastructure.Persistence.DbFactory;
using ClinicManagementSystem.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagementSystem.Infrastructure;


public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddHttpContextAccessor();
        services.AddHttpClient<Resend.IResend, Resend.ResendClient>();


        // services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        // services.AddScoped<IUnitOfWork, AdoNetUnitOfWork>();

        // services.AddScoped<IIdentityRepository, IdentityRepository>();

        // services.AddSingleton<IHasher, Hasher>();
        // services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // services.AddHttpContextAccessor();
        // services.AddScoped<ICurrentUserService, CurrentUserService>();

        // services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        // services.AddTransient<IEmailService, EmailService>();
        // services.AddTransient<ISmsService, SmsService>();
        // services.AddTransient<IFileStorageService, FileStorageService>();


        // ── Auto Register Scoped (Repositories, UnitOfWork, Services)
        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Repository") 
                                       || t.Name.EndsWith("Service") 
                                       || t.Name.EndsWith("UnitOfWork")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // ── Auto Register Singletons (Factories, Hashers, Generators, Providers)
        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Factory") 
                                       || t.Name.EndsWith("Hasher") 
                                       || t.Name.EndsWith("Generator") 
                                       || t.Name.EndsWith("Provider")))
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        return services;
    }
}
