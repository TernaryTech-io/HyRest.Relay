using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ternary.Extensions.Logging;
using HyRest;
using HyRest.DependencyInjection;
using HyRest.Identity.Credentials;

namespace HyRest.Relay;

public static class AppConfiguration
{
    private static WebApplication _app;
    public static WebApplication Start(this WebApplication app)
    {        
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
        };
        forwardedHeadersOptions.KnownIPNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeadersOptions);
        app.UseExceptionHandler();
        app.UseHylandAuthentication("/account");
        app.AddEndpoints();
        //app.UseHttpsRedirection();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapSwagger();
        }

        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        lifetime.ApplicationStopping.Register(async () =>
        {
            TokenSource.Cancel();
            var hylandApp = app.Services.GetService<OnBaseApp>();
            if(hylandApp != null)
            {
                if (hylandApp.Session.IsActive)
                    await hylandApp.Session.DisconnectAsync();
            }

        });
        app.Run();

        return app;
    }
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);        
        builder.Services.AddProblemDetails();
        Env.Load();
        var hylandAppSettings = builder.Configuration.GetSection("HylandApp");
        var apiBase = hylandAppSettings.GetValue<string>("ApiUri") ?? string.Empty;
        var idsBase = hylandAppSettings.GetValue<string>("IdSUri") ?? string.Empty;
        var clientId = Environment.GetEnvironmentVariable("HYREST_CLIENTID");
        var clientsecret = Environment.GetEnvironmentVariable("HYREST_CLIENTSECRET");

        //builder.Services.AddAuthentication(options =>
        //{
        //    options.DefaultScheme = "cookie";
        //    options.DefaultChallengeScheme = "oidc";
        //})
        //.AddCookie("cookie", options =>
        //{
        //    options.Cookie.Name = "web";

        //    // automatically revoke refresh token at signout time
        //    options.Events.OnSigningOut = async e => { await e.HttpContext.RevokeRefreshTokenAsync(); };
        //})
        //.AddOpenIdConnect("oidc", authOptions =>
        //{
        //    authOptions.Authority = idsBase;
        //    authOptions.ClientId = clientId;
        //    authOptions.ClientSecret = clientsecret;
        //    authOptions.CallbackPath = "/authenticate";
        //    authOptions.ResponseType = "code";
        //    authOptions.SignedOutCallbackPath = "/signout-callback-oidc";
        //    authOptions.SignedOutRedirectUri = "/";
        //    authOptions.GetClaimsFromUserInfoEndpoint = true;
        //    authOptions.ResponseType = "code";
        //    authOptions.SaveTokens = true;
        //    authOptions.Scope.Clear();
        //    authOptions.Scope.Add("openid");
        //    authOptions.Scope.Add("profile");
        //    authOptions.Scope.Add("profile.onbase");
        //    authOptions.Scope.Add("evolution");
        //});

        builder.AddExternalAuthHylandApp(clientOptions =>
        {
            clientOptions.ApiBaseUrl = apiBase;
            clientOptions.IdsBaseUrl = idsBase;
            //optional, defaults are provided
            clientOptions.UseQueryMetering = hylandAppSettings.GetValue<bool>("UseQueryMetering"); //default is false
            clientOptions.DefaultLanguage = hylandAppSettings.GetValue<string>("DefaultLanguage") ?? string.Empty; ; //defaults to en-US            
        },
        authOptions =>
        {
            authOptions.Authority = idsBase;
            authOptions.ClientId = clientId;
            authOptions.ClientSecret = clientsecret;
            authOptions.CallbackPath = "/authenticate";
            authOptions.ResponseType = "code";
            authOptions.SignedOutCallbackPath = "/signout-callback-oidc";
            authOptions.SignedOutRedirectUri = "/";
            authOptions.GetClaimsFromUserInfoEndpoint = true;
            authOptions.ResponseType = "code";
            authOptions.SaveTokens = true;
            authOptions.Scope.Clear();
            authOptions.Scope.Add("openid");
            authOptions.Scope.Add("profile");
            authOptions.Scope.Add("profile.onbase");
            authOptions.Scope.Add("evolution");
        });                  

        builder.Logging.AddColorConsole()
            .SetMinimumLevel(LogLevel.Information);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.WriteIndented = true;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        builder.Services.AddSingleton(TokenSource);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        Console.CancelKeyPress += ConsoleCancelHandeler;
        AppDomain.CurrentDomain.ProcessExit += ProcessExitHandler;
        AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        _app = builder.Build();
        return _app;
    }    

    internal static IAuthenticationCredentials LoadCredentials()
    {
        Env.Load();
        var username = Environment.GetEnvironmentVariable("HYREST_USERNAME") 
            ?? throw new Exception("The username is not present in the environmental variables.");
        var password = Environment.GetEnvironmentVariable("HYREST_PASSWORD")
            ?? throw new Exception("The password is not present in the environmental variables.");
        var clientId = Environment.GetEnvironmentVariable("HYREST_CLIENTID")
            ?? throw new Exception("The client id is not present in the environmental variables.");
        var clientsecret = Environment.GetEnvironmentVariable("HYREST_CLIENTSECRET")
            ?? throw new Exception("The client secret is not present in the environmental variables.");
        return AuthenticationCredentials
        .CreateUserCredentials(
            username,
            password,
            clientId,
            clientsecret
        );
    }       
    public static CancellationTokenSource TokenSource = new CancellationTokenSource();
    private static async Task Stop() => await _app.StopAsync();
    private static void ProcessExitHandler(object? sender, EventArgs e) => Stop().Wait();        
    private static void ConsoleCancelHandeler(object? sender, ConsoleCancelEventArgs e) => Stop().Wait();
    private static void CurrentDomain_DomainUnload(object? sender, EventArgs e) => Stop().Wait();
}
