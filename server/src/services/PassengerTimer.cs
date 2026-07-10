namespace DotNetElevators;

public class PassengerTimer : BackgroundService
{
    private readonly BuildingService _buildingService;
    private readonly PassengerService _passengerService;
    private readonly ILogger<PassengerTimer> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(Building.NEW_PASSENGER_SPAWN_SPEED_SEC);

    public PassengerTimer(
        BuildingService buildingService,
        PassengerService passengerService,
        ILogger<PassengerTimer> logger)
    {
        _buildingService = buildingService;
        _passengerService = passengerService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using PeriodicTimer timer = new (Interval);

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                try
                {
                    await _passengerService.AddNewPassenger(token);
                }
                catch (OperationCanceledException)
                    when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown");
                }
            }
        }
        catch(OperationCanceledException)
        when (token.IsCancellationRequested)
        {
            
        }
    }
}