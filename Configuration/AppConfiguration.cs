namespace HyRest.Relay;

public static class AppConfiguration
{
    private static WebApplication _app;
    public static WebApplication Start(this WebApplication app)
    {            
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();
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
            var hylandApp = app.Services.GetService<HylandApp>();
            if(hylandApp != null)
            {
                if (hylandApp.IsAuthenticated && hylandApp.Session.IsActive)
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
        //builder.Services.AddAuthentication("HylandAuth")
        //    .AddScheme<HylandAuthOptions,HylandAuthenticationHandler>("HylandAuth", options =>
        //    {

        //    });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
        })
        .AddCookie()
            .AddOpenIdConnect("oidc", options =>
            {                
                options.Authority = "https://onbase.ternarytech.io/auth";
                options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
                options.ClientId = "e2507cb2-93f8-4ee5-a6ff-62a1a924f5ac";
                options.ClientSecret = "CJQ1UKCkeAIQXAvshG3rau3IfBMB";
                options.ResponseType = "code"; // Authorization Code flow
                options.SaveTokens = true; // Saves access/refresh tokens in the cookie
                options.Scope.Add("openid");
                options.Scope.Add("evolution");
                options.Scope.Add("onbaseapi"); // Specific API scope

                // The redirect URL must match the one registered with the Identity Provider
                options.CallbackPath = "/authorize";
            });
        builder.Services.AddAuthorization();           

        builder.Logging.AddColorConsole()
            .SetMinimumLevel(LogLevel.Information);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.WriteIndented = true;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        var hylandAppSettings = builder.Configuration.GetSection("HylandApp");

        builder.RegisterHylandApp(LoadCredentials(), (creds, options) =>
        {
            //required
            options.ApiBaseUrl = hylandAppSettings.GetValue<string>("ApiUri") ?? string.Empty;
            options.IdsBaseUrl = hylandAppSettings.GetValue<string>("IdSUri") ?? string.Empty;
            //optional, defaults are provided
            options.UseQueryMetering = hylandAppSettings.GetValue<bool>("UseQueryMetering"); //default is false
            options.DefaultLanguage = hylandAppSettings.GetValue<string>("DefaultLanguage") ?? string.Empty; ; //defaults to en-US
                                               //optional, a default will be created if not supplied, these are the default options
            options.ClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true, //This will be overridden to true if not set
                UseCookies = true, //This will be overridden to true if not set
                CookieContainer = new System.Net.CookieContainer() //If cookie container is not set, one will be created.
            };
        });
        //Keeps HylandApp authenticated, keeps session alive.
        builder.Services.AddHostedService<KeepAliveService>();
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
        var username = Environment.GetEnvironmentVariable("HYREST_USERNAME");
        var password = Environment.GetEnvironmentVariable("HYREST_PASSWORD");
        var clientId = Environment.GetEnvironmentVariable("HYREST_CLIENTID");
        var clientsecret = Environment.GetEnvironmentVariable("HYREST_CLIENTSECRET");
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
