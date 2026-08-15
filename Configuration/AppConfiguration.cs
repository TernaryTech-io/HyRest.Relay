using DotNetEnv;
using HyRest.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ternary.Extensions.Logging;

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
        app.UseHylandAuthentication();
        app.AddEndpoints();

        //var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        //lifetime.ApplicationStopping.Register(async () =>
        //{
        //    TokenSource.Cancel();
        //    var hylandApp = app.Services.GetService<OnBaseApp>();
        //    if(hylandApp != null)
        //    {
        //        if (hylandApp.Session.IsActive)
        //            await hylandApp.Session.DisconnectAsync();
        //    }

        //});
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
               

        builder.AddOpenIdHylandApp<OnBaseScopedApp>(credentials =>
        {
            credentials.ClientId = clientId;
            credentials.ClientSecret = clientsecret;
            //These Represent the default
            credentials.CallbackPath = "/authenticate";
            credentials.SignedOutCallbackPath = "/signout-callback-oidc";
            credentials.SignedOutRedirectUri = "/";
            //Not Required unless your scope differs from below:
            //credentials.ClearScope();
            //credentials.AddScope("openid");
            //credentials.AddScope("profile");
            //credentials.AddScope("profile.onbase");
            //credentials.AddScope("evolution");
        },
        clientOptions =>
        {
            clientOptions.ApiBaseUrl = apiBase;
            clientOptions.IdsBaseUrl = idsBase;
            //optional, defaults are provided
            clientOptions.UseQueryMetering = hylandAppSettings.GetValue<bool>("UseQueryMetering"); //default is false
            clientOptions.DefaultLanguage = hylandAppSettings.GetValue<string>("DefaultLanguage") ?? string.Empty; ; //defaults to en-US
            clientOptions.RequestTimeOut = 120;
        });                  

        builder.Logging.AddConsole()
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
        //builder.Services.AddSwaggerGen();
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
