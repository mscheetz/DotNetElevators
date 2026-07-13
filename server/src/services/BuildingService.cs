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

    public async Task ElevatorArrivesAtFloor(int elevatorNumber, int destinationFloor)
    {
        var elevator = Building.Elevators[elevatorNumber];

        var variance = elevator.ElevatorDirection == Direction.UP ? 1 : -1;
        var floor = elevator.CurrentFloor + variance;

        if (await GoToVIPFloor(elevator, floor))
        {
            return;
        }

        //var pendingPassengers = Building.Floors[floor].QueuedPassengers.Where(p => p.Direction == elevator.ElevatorDirection).ToList();

        var departedPassengers = elevator.ArriveAtFloor(floor, Building.Floors[floor].QueuedPassengers);
        Building.Floors[floor].PassengersDeparted(departedPassengers);

        var vips = elevator.Passengers.Where(p => p.VIP);
        
        if (vips.Any())
        {
            destinationFloor = elevator.ElevatorDirection == Direction.UP 
                                                    ? vips.Min(p => p.Destination)
                                                    : vips.Max(p => p.Destination);

            _logger.LogInformation("[{Elevator}]: VIP entered elevator, new destination is floor {Destination}", elevatorNumber, destinationFloor);            
        }

        if (floor != destinationFloor)
        {
            var queueItem = new QueueItem(elevatorNumber, destinationFloor);
            await _queueManager.AddToQueueAsync(queueItem);
        }
        else if (elevator.Passengers.Any())
        {
            var queueItem = new QueueItem(elevatorNumber, elevator.DestinationFloor!.Value);
            await _queueManager.AddToQueueAsync(queueItem);
        }
    }

    private async Task<bool> GoToVIPFloor(Elevator elevator, int floor)
    {
        var vips = elevator.Passengers.Where(p => p.VIP);

        if (vips.Any())
        {
            var closestDestination = elevator.ElevatorDirection == Direction.UP 
                                                    ? vips.Min(p => p.Destination)
                                                    : vips.Max(p => p.Destination);

            if (closestDestination != floor)
            {
                elevator.CurrentFloor = floor;
                elevator.DestinationFloor = closestDestination;

                _logger.LogInformation("[{Elevator}]: VIP destination {Destination}; skipping this floor ({Floor})", elevator.Id, closestDestination, floor);
                var queueItem = new QueueItem(elevator.Id, closestDestination);
                await _queueManager.AddToQueueAsync(queueItem);

                return true;
            }
        }

        return false;
    }

    public async Task<int?> CallElevator(int floor, Direction requestedDirection)
    {
        var idleElevators = Building.Elevators.Values.Where(e => e.DestinationFloor is null).ToList();
        
        if (!idleElevators.Any())
        {
            _logger.LogInformation("No idle elevators to call");
            return null;
        }

        var willPass = Building.Elevators.Values.Any(e =>
            e.ElevatorDirection == requestedDirection &&
            (
                (e.ElevatorDirection == Direction.UP && e.CurrentFloor < floor && e.DestinationFloor >= floor)
                ||
                (e.ElevatorDirection == Direction.DOWN && e.CurrentFloor > floor && e.DestinationFloor <= floor)
            )
            ||
            (e.CurrentFloor == floor && e.DoorOpen));

        if (willPass)
        {
            _logger.LogInformation("[E] Elevator enroute to {Floor}", floor);
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

        _logger.LogInformation("[{Elevator}] **** Elevator now in service! ****", bestElevator.Id);

        return bestElevator.Id;
    }
}