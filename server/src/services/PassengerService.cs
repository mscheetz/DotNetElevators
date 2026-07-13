namespace DotNetElevators;

public class PassengerService
{
    private readonly BuildingService _buildingService;
    private readonly ILogger<PassengerService> _logger;

    public PassengerService(
        BuildingService buildingService,
        ILogger<PassengerService> logger)
    {
        _buildingService = buildingService;
        _logger = logger;
    }

    public async Task AddNewPassenger(CancellationToken token)
    {
        var delay = Building.NEW_PASSENGER_SPAWN_SPEED_SEC * 1000;
        await Task.Delay(delay, token);

        var floor = HelperService.GetRandomFloor();
        var destination = HelperService.GetRandomFloor(floor);
        var vipStatus = HelperService.GetRandomizedVIP();

        var passenger = new Passenger(floor, destination);
        passenger.VIP = vipStatus;

        await AddNewPassenger(passenger);
    }

    public async Task AddNewPassenger(Passenger passenger)
    {
        Building.Floors[passenger.Source].QueuePassenger(passenger);

        _logger.LogInformation("--{Floor}-- New Passenger on floor {Floor} -> {Destination}", passenger.Source, passenger.Source, passenger.Destination);

        var calledElevator = await _buildingService.CallElevator(passenger.Source, passenger.Direction);

        if (calledElevator.HasValue)
        {
            _logger.LogInformation("[{Elevator}] Called Elevator {Elevator} to floor {Floor}", calledElevator, calledElevator, passenger.Source);
        }
        else
        {
            _logger.LogInformation("An elevator is enroute to floor {Floor}", passenger.Source);
        }        
    }
}