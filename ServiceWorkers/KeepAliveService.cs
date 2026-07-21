using Ternary.HyRest;

namespace HyRest.Relay;

public class KeepAliveService : BackgroundService
{
    private readonly ILogger<KeepAliveService> _logger;
    private readonly HylandApp _app;
    private readonly CancellationTokenSource _cts;
    public KeepAliveService(ILogger<KeepAliveService> logger, HylandApp app, CancellationTokenSource cts)
    {
        _logger = logger;
        _app = app;
        _cts = cts;
        
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!_cts.IsCancellationRequested)
        {
            if(_cts.IsCancellationRequested)
            {
                if (_app.IsAuthenticated && _app.Session.IsActive)
                    await _app.Session.DisconnectAsync();
                break;
            }

            if (!_app.IsAuthenticated)
                await _app.AuthenticateAsync();
            if (!_app.Session.IsActive)
                await _app.Session.InitiateAsync();
            await _app.Session.HeartbeatAsync();

            await Task.Delay(TimeSpan.FromMinutes(4), _cts.Token);
        }
        
    }    
}