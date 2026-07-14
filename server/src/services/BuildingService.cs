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

    public async Task ElevatorArrivesAtFloorV1(int elevatorNumber, int destinationFloor)
    {
        var elevator = Building.Elevators[elevatorNumber];

        var oldFloor = elevator.CurrentFloor;
        var variance = elevator.ElevatorDirection == Direction.UP ? 1 : -1;
        var floor = elevator.CurrentFloor + variance;

        if (!Building.Floors[floor].IsActive)
        {
            elevator.CurrentFloor = floor;
            await BroadcastElevator(elevator);
            await BroadcastFloor(oldFloor);

            if (floor != destinationFloor)
            {
                var queueItem = new QueueItem(elevatorNumber, destinationFloor);
                await _queueManager.AddToQueueAsync(queueItem);
            }

            return;
        }

        if (await GoToVIPFloor(elevator, floor, oldFloor))
        {
            return;
        }

        if(!Building.Floors[floor].QueuedPassengers.Any(p => p.Direction == elevator.ElevatorDirection)
            && !elevator.Passengers.Any(p => p.Destination == floor) )
        {
            _logger.LogInformation("[{Elevator}] : No passengers getting off and no queued passengers on {Floor}", elevatorNumber, floor);
            elevator.CurrentFloor = floor;
            await BroadcastElevator(elevator);
            await BroadcastFloor(oldFloor);
            await BroadcastFloor(floor);

            var queueItem = new QueueItem(elevatorNumber, destinationFloor);
            await _queueManager.AddToQueueAsync(queueItem);

            return;            
        }

        var departedPassengers = elevator.ArriveAtFloor(floor, Building.Floors[floor].QueuedPassengers);

        Building.Floors[floor].PassengersDeparted(departedPassengers);

        if (elevator.ElevatorDirection is null && Building.Floors.Values.Any(f => f.QueuedPassengers.Any()))
        {
            var queuedFloors = Building.Floors.Values.Where(f => f.QueuedPassengers.Any());
            var queuedCounts = queuedFloors.SelectMany(f => f.QueuedPassengers)
                                                .GroupBy(p => p.Direction)
                                                .ToDictionary(k => k.Key, v => v.Count());            

            var up = queuedCounts.GetValueOrDefault(Direction.UP);
            var down = queuedCounts.GetValueOrDefault(Direction.DOWN);

            _logger.LogInformation("-- Queued Passenger Counts: UP {UP} DOWN {DOWN} --", up, down);

            var groundFloorCount = Building.Elevators.Values.Count(e => e.DestinationFloor == 1);
            var topFloorCount = Building.Elevators.Values.Count(e => e.DestinationFloor == Building.MAX_FLOOR);

            var target = up >= down && topFloorCount == 0 ? Building.MAX_FLOOR
                       : down > up && groundFloorCount == 0 ? 1
                       : up > down ? Building.MAX_FLOOR
                       : 1;

            if (target != floor)
            {
                elevator.DestinationFloor = target;
                elevator.ElevatorDirection = target > floor ? Direction.UP : Direction.DOWN;

                _logger.LogInformation("[{Elevator}]: Elevator is empty, heading to {Floor} to pick up queued passengers", elevatorNumber, elevator.DestinationFloor);
                await BroadcastElevator(elevator);

                destinationFloor = elevator.DestinationFloor!.Value;
            }
            else
            {
                var reverseTarget = target == Building.MAX_FLOOR ? 1 : Building.MAX_FLOOR;

                _logger.LogInformation("[{Elevator}]: Elevator is empty at terminal floor, reversing to {Floor}", elevatorNumber, reverseTarget);
                elevator.DestinationFloor = reverseTarget;
                elevator.ElevatorDirection = reverseTarget > floor ? Direction.UP : Direction.DOWN;
                await BroadcastElevator(elevator);

                destinationFloor = reverseTarget;
            }
        }

        await BroadcastFloor(oldFloor);
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

    public async Task ElevatorArrivesAtFloor(int elevatorNumber, int destinationFloor)
    {
        var elevator = Building.Elevators[elevatorNumber];
        var oldFloor = elevator.CurrentFloor;
        var variance = elevator.ElevatorDirection == Direction.UP ? 1 : -1;
        var floor = elevator.CurrentFloor + variance;

        // Floor inactive — skip
        if (!Building.Floors[floor].IsActive)
        {
            elevator.CurrentFloor = floor;
            await BroadcastElevator(elevator);
            await BroadcastFloor(oldFloor);

            if (floor != destinationFloor)
            {
                var queueItem = new QueueItem(elevatorNumber, destinationFloor);
                await _queueManager.AddToQueueAsync(queueItem);
            }
            return;
        }

        // VIP on board — delegate to VIP handler (it broadcasts and enqueues)
        if (await GoToVIPFloor(elevator, floor, oldFloor))
        {
            return;
        }

        var queuedSameDir = Building.Floors[floor].QueuedPassengers.Where(p => p.Direction == elevator.ElevatorDirection).ToList();
        var passengersGettingOff = elevator.Passengers.Any(p => p.Destination == floor);

        // CONTINUE — no reason to stop at this floor
        if (!passengersGettingOff && queuedSameDir.Count == 0)
        {
            elevator.CurrentFloor = floor;
            await BroadcastElevator(elevator);
            await BroadcastFloor(oldFloor);
            await BroadcastFloor(floor);

            if (floor != destinationFloor)
            {
                var queueItem = new QueueItem(elevatorNumber, destinationFloor);
                await _queueManager.AddToQueueAsync(queueItem);
            }
            return;
        }

        // STOP — open doors, let off, pick up
        var previousDirection = elevator.ElevatorDirection;
        var departedPassengers = elevator.ArriveAtFloor(floor, Building.Floors[floor].QueuedPassengers);
        Building.Floors[floor].PassengersDeparted(departedPassengers);

        if (!elevator.Passengers.Any())
        {
            // EMPTY AFTER STOP — find work or idle
            var queuedAnywhere = Building.Floors.Values
                .SelectMany(f => f.QueuedPassengers)
                .ToList();

            await BroadcastElevator(elevator);
            await BroadcastFloor(oldFloor);
            await BroadcastFloor(floor);

            if (queuedAnywhere.Count == 0)
            {
                elevator.DestinationFloor = null;
                elevator.ElevatorDirection = null;

                await BroadcastElevator(elevator);
                await BroadcastFloor(oldFloor);
                await BroadcastFloor(floor);

                return;
            }

            var dir = previousDirection ?? Direction.UP;
            var sameDirQueue = queuedAnywhere.Where(p => p.Direction == dir).ToList();
            var oppDirQueue = queuedAnywhere.Where(p => p.Direction != dir).ToList();

            var targetFloor = sameDirQueue.Count > 0
                ? (dir == Direction.UP
                    ? sameDirQueue.Max(p => p.Source)
                    : sameDirQueue.Min(p => p.Source))
                : (dir == Direction.UP
                    ? oppDirQueue.Min(p => p.Source)
                    : oppDirQueue.Max(p => p.Source));

            if (targetFloor == floor)
            {
                return;
            }

            elevator.DestinationFloor = targetFloor;
            elevator.ElevatorDirection = targetFloor > floor ? Direction.UP : Direction.DOWN;

            _logger.LogInformation("[{Elevator}]: Empty after stop, heading to floor {Floor}", elevatorNumber, targetFloor);
            await BroadcastElevator(elevator);

            var queueItem = new QueueItem(elevatorNumber, targetFloor);
            await _queueManager.AddToQueueAsync(queueItem);
            return;
        }

        // HAS PASSENGERS — continue toward destination
        await BroadcastElevator(elevator);
        await BroadcastFloor(oldFloor);
        await BroadcastFloor(floor);

        if (elevator.DestinationFloor.HasValue && elevator.DestinationFloor != floor)
        {
            var queueItem = new QueueItem(elevatorNumber, elevator.DestinationFloor.Value);
            await _queueManager.AddToQueueAsync(queueItem);
        }
    }

    private async Task<bool> GoToVIPFloor(Elevator elevator, int floor, int oldFloor)
    {
        var vips = elevator.Passengers.Where(p => p.VIP);

        if (vips.Any())
        {
            var closestDestination = elevator.ElevatorDirection == Direction.UP 
                                                    ? vips.Min(p => p.Destination)
                                                    : vips.Max(p => p.Destination);

            if (closestDestination != floor)
            {
                await BroadcastFloor(oldFloor);

                elevator.CurrentFloor = floor;
                elevator.DestinationFloor = closestDestination;

                await BroadcastElevator(elevator);
                await BroadcastFloor(floor);
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
        bestElevator.ElevatorDirection = bestElevator.CurrentFloor > floor ? Direction.DOWN : Direction.UP;

        await BroadcastElevator(bestElevator);
        await BroadcastFloor(floor);

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
        if (!Building.Elevators.TryGetValue(elevatorId, out var elevator))
        {
            return false;
        }

        elevator.IsActive = !elevator.IsActive;

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