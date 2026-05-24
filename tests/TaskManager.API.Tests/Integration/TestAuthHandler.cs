using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TaskManager.API.Tests.Integration;

/// <summary>
/// A test authentication handler that authenticates every request as a fixed user. Registered by
/// <see cref="AiEndpointsIntegrationTests"/> so the <c>[Authorize]</c> AI endpoints can be exercised
/// without minting real JWTs, keeping the integration tests deterministic and self-contained.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// The name of the test authentication scheme.
    /// </summary>
    public const string SchemeName = "Test";

    /// <summary>
    /// Requests carrying this header are authenticated; requests without it remain anonymous, so the
    /// same host can exercise both an authenticated endpoint and an unauthenticated 401 path.
    /// </summary>
    public const string AuthHeaderName = "X-Test-Auth";

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAuthHandler"/> class.
    /// </summary>
    /// <param name="options">The options monitor.</param>
    /// <param name="logger">The logger factory.</param>
    /// <param name="encoder">The URL encoder.</param>
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(AuthHeaderName))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "integration-test-user")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
