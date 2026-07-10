using Microsoft.Extensions.Logging;

namespace DotNetElevators;

public class BuildingService
{
    private readonly QueueManager _queueManager;
    private readonly ILogger<BuildingService> _logger;

    public BuildingService(
        QueueManager queueManager,
        ILogger<BuildingService> logger)
    {
        _queueManager = queueManager;
        _logger = logger;
        ResetBuilding();
    }

    public void ResetBuilding()
    {
        Building.Floors.Clear();
        for (var i = Building.MIN_FLOOR; i <= Building.MAX_FLOOR; i++)
        {
            Building.Floors[i] = new Floor(i);
        }
        _logger.LogInformation($"{Building.Floors.Count} Floors setup");

        Building.Elevators.Clear();
        for (var i = 1; i <= Building.ELEVATOR_COUNT; i++)
        {
            Building.Elevators[i] = new Elevator(i);
        }
        _logger.LogInformation($"{Building.Elevators.Count} Elevators ready for service");
    }

    public async Task<int?> ElevatorArrivesAtFloor(int elevatorNumber, int destinationFloor)
    {
        var elevator = Building.Elevators[elevatorNumber];

        var variance = elevator.ElevatorDirection == Direction.UP ? 1 : -1;
        var floor = elevator.CurrentFloor + variance;

        var pendingPassengers = Building.Floors[floor].QueuedPassengers.Where(p => p.Direction == elevator.ElevatorDirection).ToList();

        var departedPassengers = elevator.ArriveAtFloor(floor, pendingPassengers);
        Building.Floors[floor].PassengersDeparted(departedPassengers);

        if (floor != destinationFloor)
        {
            var queueItem = new QueueItem(elevatorNumber, destinationFloor);
            await _queueManager.AddToQueueAsync(queueItem);
        }

        return elevator.Passengers.Any() ? elevator.DestinationFloor : null;
    }

    public async Task<int?> CallElevator(int floor)
    {
        var idleElevators = Building.Elevators.Values.Where(e => e.DestinationFloor is null).ToList();
        
        if (!idleElevators.Any())
        {
            _logger.LogInformation("No idle elevators to call");
            return null;
        }

        if (Building.Elevators.Values.Any(e => 
            (e.CurrentFloor < floor && e.ElevatorDirection == Direction.UP)
            || (e.CurrentFloor > floor && e.ElevatorDirection == Direction.DOWN)
            || e.CurrentFloor == floor && e.DoorOpen))
        {
            _logger.LogInformation("Closer elevator currently enroute to {Floor}", floor);
            return null;
        }

        var bestElevator = idleElevators.MinBy(e => Math.Abs(e.CurrentFloor - floor));

        if (bestElevator is null)
        {
            return null;
        }

        bestElevator.DestinationFloor = floor;
        bestElevator.ElevatorDirection = bestElevator.CurrentFloor > floor ? Direction.DOWN : Direction.UP;;

        var queueItem = new QueueItem(bestElevator.Id, floor);
        
        await _queueManager.AddToQueueAsync(queueItem);

        return bestElevator.Id;
    }
}