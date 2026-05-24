using Anthropic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Repositories;
using TaskManager.Infrastructure.Services;
using TaskManager.Infrastructure.Services.AiAssistant;

namespace TaskManager.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        AddAiAssistant(services, configuration);

        return services;
    }

    /// <summary>
    /// Registers the AI assistant. When an Anthropic API key is available (via configuration or the
    /// <c>ANTHROPIC_API_KEY</c> environment variable) the live <see cref="AnthropicAiAssistant"/> is
    /// registered along with an <see cref="IChatClient"/>; otherwise a <see cref="DisabledAiAssistant"/>
    /// is registered so the feature degrades gracefully without breaking the rest of the API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    private static void AddAiAssistant(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        var aiOptions = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();

        var apiKey = configuration["ANTHROPIC_API_KEY"]
            ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddScoped<IAiAssistant, DisabledAiAssistant>();
            return;
        }

        services.AddChatClient(_ =>
            new AnthropicClient { ApiKey = apiKey }
                .AsIChatClient(aiOptions.Model)
                .AsBuilder()
                .Build());

        services.AddScoped<IAiAssistant, AnthropicAiAssistant>();
    }
}
