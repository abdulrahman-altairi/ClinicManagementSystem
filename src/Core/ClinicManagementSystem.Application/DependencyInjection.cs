using ClinicManagementSystem.Application.Common.Options;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace ClinicManagementSystem.Application;


public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        // services.AddScoped<IAuthServices, AuthServices>();

        services.Scan(scan => scan
        .FromAssemblies(typeof(DependencyInjection).Assembly) 
        .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Services") || c.Name.EndsWith("Service")))
        .AsImplementedInterfaces()
        .WithScopedLifetime());

        // services.AddScoped<IValidator<RegisterUserRequestDto>,    RegisterUserRequestValidator>();
        // services.AddScoped<IValidator<LoginRequestDto>,           LoginRequestValidator>();
        // services.AddScoped<IValidator<RefreshTokenRequestDto>,    RefreshTokenRequestValidator>();
        // services.AddScoped<IValidator<ChangePasswordRequestDto>,  ChangePasswordRequestValidator>();


        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}