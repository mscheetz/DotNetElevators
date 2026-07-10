using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetElevators;

public class PassengerService : BackgroundService
{
    private readonly BuildingService _buildingService;
    private readonly ILogger<PassengerService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(Building.NEW_PASSENGER_SPAWN_SPEED_SEC);

    public PassengerService(
        BuildingService buildingService,
        ILogger<PassengerService> logger)
    {
        _buildingService = buildingService;
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
                    await AddNewPassenger(token);
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

    private async Task AddNewPassenger(CancellationToken token)
    {
        var delay = Building.NEW_PASSENGER_SPAWN_SPEED_SEC * 1000;
        await Task.Delay(delay, token);

        var floor = Random.Shared.Next(Building.MIN_FLOOR, Building.MAX_FLOOR + 1);
        var destination = GetDestination(floor);

        var passenger = new Passenger(floor, destination);

        Building.Floors[floor].QueuePassenger(passenger);

        _logger.LogInformation("--{Floor}-- New Passenger on floor {Floor} -> {Destination}", floor, floor, destination);

        var calledElevator = await _buildingService.CallElevator(floor, passenger.Direction);

        if (calledElevator.HasValue)
        {
            _logger.LogInformation("[{Elevator}] Called Elevator {Elevator} to floor {Floor}", calledElevator, calledElevator, floor);
        }
        else
        {
            _logger.LogInformation("An elevator is enroute to floor {Floor}", floor);
        }
    }

    private int GetDestination(int startingFloor)
    {
        int destination = 0;
        while (true)
        {
            destination = Random.Shared.Next(Building.MIN_FLOOR, Building.MAX_FLOOR + 1);

            if (destination != startingFloor)
            {
                break;
            }
        }

        return destination;
    }
}