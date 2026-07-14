namespace DotNetElevators;

public class BuildingService
{
    private readonly BuildingBroadcastService _broadcastService;
    private readonly QueueManager _queueManager;
    private readonly ILogger<BuildingService> _logger;

    public BuildingService(
        BuildingBroadcastService buildingBroadcastService,
        QueueManager queueManager,
        ILogger<BuildingService> logger)
    {
        _broadcastService = buildingBroadcastService;
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

        if (!Building.Floors[floor].IsActive)
        {
            
        }

        if (await GoToVIPFloor(elevator, floor))
        {
            return;
        }

        var departedPassengers = elevator.ArriveAtFloor(floor, Building.Floors[floor].QueuedPassengers);

        Building.Floors[floor].PassengersDeparted(departedPassengers);

        await BroadcastFloor(floor);

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
            await BroadcastElevator(elevator);
            var queueItem = new QueueItem(elevatorNumber, destinationFloor);
            await _queueManager.AddToQueueAsync(queueItem);
        }
        else if (elevator.Passengers.Any())
        {
            await BroadcastElevator(elevator);
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

                await BroadcastElevator(elevator);
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
        var idleElevators = Building.Elevators.Values.Where(e => e.DestinationFloor is null && e.IsActive).ToList();
        
        if (!idleElevators.Any())
        {
            _logger.LogInformation("No idle elevators to call");
            return null;
        }

        var willPass = Building.Elevators.Values.Any(e =>
            e.ElevatorDirection == requestedDirection && e.IsActive &&
            (
                (e.ElevatorDirection == Direction.UP && e.CurrentFloor < floor && e.DestinationFloor >= floor)
                ||
                (e.ElevatorDirection == Direction.DOWN && e.CurrentFloor > floor && e.DestinationFloor <= floor)
            )
            ||
            (e.CurrentFloor == floor && e.DoorOpen && e.IsActive));

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

        await BroadcastElevator(bestElevator);

        var queueItem = new QueueItem(bestElevator.Id, floor);        
        await _queueManager.AddToQueueAsync(queueItem);

        _logger.LogInformation("[{Elevator}] **** Elevator now in service! ****", bestElevator.Id);

        return bestElevator.Id;
    }

    public async Task<int> AddFloor()
    {
        var newFloorNumber = Building.Floors.Keys.Max() + 1;
        var newFloor = new Floor(newFloorNumber);

        Building.MAX_FLOOR = newFloorNumber;
        Building.Floors.TryAdd(newFloorNumber, newFloor);

        await BroadcastFloor(newFloorNumber);

        return newFloorNumber;
    }

    public async Task<bool> ToggleFloorStatus(int floorNumber)
    {
        var topFloor = Building.Floors.Keys.Max();
        var reverseDown = topFloor == floorNumber || topFloor == (floorNumber + 1);
        var reverseUp = floorNumber == 1 || floorNumber == 2;

        var queuedPassengers = Building.Floors.Values.SelectMany(f => f.QueuedPassengers).Where(p => p.Destination == floorNumber);

        foreach (var passenger in queuedPassengers)
        {
            passenger.Destination--;
        }

        var elevators = Building.Elevators.Values.Where(e => e.CurrentFloor == floorNumber || e.DestinationFloor == floorNumber);

        foreach (var elevator in elevators)
        {
            if (elevator.CurrentFloor == floorNumber)
            {
                if (reverseDown && elevator.ElevatorDirection == Direction.UP)
                {
                    elevator.CurrentFloor--;
                    elevator.ElevatorDirection = Direction.DOWN;
                    _logger.LogInformation("[{Elevator}]: Moved to new floor and changed direction", elevator.Id);
                }
                else if (reverseUp && elevator.ElevatorDirection == Direction.DOWN)
                {
                    elevator.CurrentFloor++;
                    elevator.ElevatorDirection = Direction.UP;
                    _logger.LogInformation("[{Elevator}]: Moved to new floor and changed direction", elevator.Id);
                }
                else
                {                    
                    elevator.CurrentFloor--;
                    _logger.LogInformation("[{Elevator}]: Moved to new floor", elevator.Id);
                }
            }
            if (elevator.DestinationFloor == floorNumber)
            {
                elevator.DestinationFloor = elevator.DestinationFloor == 1 ? elevator.DestinationFloor++ : elevator.DestinationFloor--;
                _logger.LogInformation("[{Elevator}]: Destination changed to new floor", elevator.Id);
            }
            var passengers = elevator.Passengers.Where(p => p.Destination == floorNumber);

            foreach (var passenger in passengers)
            {
                passenger.Destination--;
            }

            await BroadcastElevator(elevator);
        }

        var currentStatus = Building.Floors[floorNumber].IsActive;

        Building.Floors[floorNumber].IsActive = !currentStatus;

        return true;
    }

    public async Task<bool> ToggleElevatorStatus(int elevatorId)
    {
        if (Building.Elevators.TryGetValue(elevatorId, out Elevator? elevator) && elevator is null)
        {
            return false;
        }

        elevator!.IsActive = !elevator.IsActive;

        return true;
    }

    public async Task<int> AddElevator()
    {
        var elevatorId = Building.Elevators.Keys.Max() + 1;

        var elevator = new Elevator(elevatorId);

        Building.Elevators.TryAdd(elevatorId, elevator);

        await BroadcastElevator(elevator);

        return elevatorId;
    }

    public Task BroadcastElevator(Elevator elevator)
    {
        return _broadcastService.BroadcastElevator(new ElevatorDTO(elevator));
    }

    public Task BroadcastFloor(int floorNumber)
    {
        return _broadcastService.BroadcastFloor(floorNumber);
    }
}