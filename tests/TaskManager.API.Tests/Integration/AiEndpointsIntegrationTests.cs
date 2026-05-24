using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Infrastructure.Services.AiAssistant;

namespace TaskManager.API.Tests.Integration;

/// <summary>
/// End-to-end integration tests for the AI endpoints booted via <see cref="WebApplicationFactory{TEntryPoint}"/>
/// with NO Anthropic API key configured. These tests exercise the full HTTP pipeline (routing,
/// authorization, middleware) and therefore catch the deferred-throw bug that unit tests mask by
/// draining the enumerable directly: they assert that the missing-key path produces a clean HTTP 503
/// set BEFORE any response body is flushed. No real network/API calls occur (the disabled assistant
/// throws synchronously).
/// </summary>
public class AiEndpointsIntegrationTests : IClassFixture<AiEndpointsIntegrationTests.NoKeyApiFactory>
{
    private readonly NoKeyApiFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiEndpointsIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">The application factory configured with no AI key.</param>
    public AiEndpointsIntegrationTests(NoKeyApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task StreamProjectSummary_Should_Return503_WhenAiKeyNotConfigured()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act — authenticated (header present) so we reach the handler, which throws eagerly.
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/ai/projects/{Guid.NewGuid()}/summary");
        request.Headers.Add(TestAuthHandler.AuthHeaderName, "true");
        var response = await client.SendAsync(request);

        // Assert — 503 set before headers flush; this is the acceptance criterion.
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task DraftTask_Should_Return503_WhenAiKeyNotConfigured()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/ai/tasks/draft")
        {
            Content = JsonContent.Create(new { input = "pay the suppliers on Friday, urgent" })
        };
        request.Headers.Add(TestAuthHandler.AuthHeaderName, "true");
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task NonAiEndpoint_Should_Return401_WhenUnauthenticated()
    {
        // Arrange — prove the rest of the API pipeline is unaffected by the missing AI key. A
        // protected non-AI endpoint, hit WITHOUT the test-auth header, must still enforce auth (401).
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// A <see cref="WebApplicationFactory{TEntryPoint}"/> that boots the API with no Anthropic API key
    /// (so the <see cref="DisabledAiAssistant"/> is registered) and a header-gated test authentication
    /// scheme in place of JWT bearer.
    /// </summary>
    public class NoKeyApiFactory : WebApplicationFactory<Program>
    {
        /// <inheritdoc/>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Run as Production so dev-only branches (Swagger) stay inert and configuration is explicit.
            builder.UseEnvironment("Production");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                // Highest-priority overrides: guarantee no API key is visible to DI and provide the
                // minimal JWT config Program.cs requires at startup (the test scheme replaces bearer).
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ANTHROPIC_API_KEY"] = null,
                    ["Jwt:Secret"] = "IntegrationTestSecretKeyThatIsLongEnoughForHmacSha256SigningAlgorithm!",
                    ["Jwt:Issuer"] = "TaskManager.Tests",
                    ["Jwt:Audience"] = "TaskManager.Tests",
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=integration_tests;Username=test;Password=test"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                // Force the test authentication scheme as the default so [Authorize] (no explicit
                // scheme) resolves to it instead of JWT bearer.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                // Be explicit and deterministic: regardless of the host machine's environment variables,
                // the assistant is the disabled (no-key) implementation, so no outbound calls happen.
                services.RemoveAll<IAiAssistant>();
                services.AddScoped<IAiAssistant, DisabledAiAssistant>();
            });
        }
    }
}
