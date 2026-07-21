using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace HyRest.Relay;

public class HylandAuthOptions : AuthenticationSchemeOptions
{

}

public class HylandAuthenticationHandler : AuthenticationHandler<HylandAuthOptions>
{
    public HylandAuthenticationHandler(IOptionsMonitor<HylandAuthOptions> options,
                             ILoggerFactory logger, UrlEncoder encoder,
                             ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        // Example: Basic Auth parsing
        var credentials = Encoding.UTF8.GetString(
            Convert.FromBase64String(authHeader.ToString().Split(' ')[1])
        ).Split(':');

        var clientId = credentials[0];
        var clientSecrect = credentials[1];

        // Validate against database/service
        var isValid = await ValidateCredentialsAsync(clientId, clientSecrect);

        if (!isValid)
            return AuthenticateResult.Fail("Invalid credentials");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, clientId),
            new Claim("client_id", clientId),
            new Claim(ClaimTypes.Role, "ApiClient")
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private Task<bool> ValidateCredentialsAsync(string clientId, string secret)
    {
        Env.Load();
        var clientGuid = Environment.GetEnvironmentVariable("HYREST_CLIENTID");
        var clientsecret = Environment.GetEnvironmentVariable("HYREST_CLIENTSECRET");
        return Task.FromResult(clientGuid == clientId && clientsecret == secret);
    }
}