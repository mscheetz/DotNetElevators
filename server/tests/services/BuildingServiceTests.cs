using DotNetElevators;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetElevators.Tests;

public class BuildingServiceTests
{
    private readonly BuildingService _service;

    public BuildingServiceTests()
    {
        Building.ELEVATOR_COUNT = 1;
        Building.Floors.Clear();
        Building.Elevators.Clear();

        var hub = new MockHub<BuildingHub>();
        var broadcast = new BuildingBroadcastService(hub);
        var queueManager = new QueueManager();
        _service = new BuildingService(broadcast, queueManager, NullLogger<BuildingService>.Instance);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_DisembarksPassengers()
    {
        var elevator = new Elevator(1);
        elevator.CurrentFloor = 2;
        elevator.DestinationFloor = 3;
        elevator.ElevatorDirection = Direction.UP;
        elevator.Passengers = new List<Passenger>
        {
            new(1, 6),
            new(1, 4),
            new(1, 3),
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(4, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.Equal(2, elevator.Passengers.Count);
        Assert.DoesNotContain(elevator.Passengers, p => p.Destination == 3);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_DisembarksPassengers_PickupPassengers()
    {
        var floor = Building.Floors[3];
        floor.QueuedPassengers = new List<Passenger>
        {
            new (3, 5),
        };

        var elevator = new Elevator(1);
        elevator.CurrentFloor = 2;
        elevator.DestinationFloor = 3;
        elevator.ElevatorDirection = Direction.UP;
        elevator.Passengers = new List<Passenger>
        {
            new(1, 6),
            new(1, 4),
            new(1, 3),
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        var actualFloor = Building.Floors[3];

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(4, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.Equal(3, elevator.Passengers.Count);
        Assert.Equal(0, actualFloor.QueuedPassengers.Count);
        Assert.DoesNotContain(elevator.Passengers, p => p.Destination == 3);
    }

    /// <summary>
    /// Disembark all passengers
    /// No queued passengers anywhere
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_DisembarksAllPassengers_GoIdle()
    {
        var elevator = new Elevator(1);
        elevator.CurrentFloor = 2;
        elevator.DestinationFloor = 3;
        elevator.ElevatorDirection = Direction.UP;
        elevator.Passengers = new List<Passenger>
        {
            new(1, 3),
            new(1, 3),
            new(1, 3),
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(null, elevator.DestinationFloor);
        Assert.Equal(null, elevator.ElevatorDirection);
        Assert.Equal(0, elevator.Passengers.Count);
    }

    /// <summary>
    /// Disembark all passengers
    /// queued passengers on 1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_DisembarksAllPassengers_GetQueued()
    {
        var floor = Building.Floors[1];
        floor.QueuedPassengers = new List<Passenger>
        {
            new (1, 5),
        };

        var elevator = new Elevator(1);
        elevator.CurrentFloor = 2;
        elevator.DestinationFloor = 3;
        elevator.ElevatorDirection = Direction.UP;
        elevator.Passengers = new List<Passenger>
        {
            new(1, 3),
            new(1, 3),
            new(1, 3),
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(1, elevator.DestinationFloor);
        Assert.Equal(Direction.DOWN, elevator.ElevatorDirection);
        Assert.Equal(0, elevator.Passengers.Count);
    }

    /// <summary>
    /// No passengers
    /// queued passengers on 1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_NoPassengers_GetQueued01()
    {
        var floor = Building.Floors[1];
        floor.QueuedPassengers = new List<Passenger>
        {
            new (1, 5),
        };

        var elevator = new Elevator(1);
        elevator.CurrentFloor = 3;
        elevator.DestinationFloor = 1;
        elevator.ElevatorDirection = Direction.DOWN;
        elevator.Passengers = new();

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(2, elevator.CurrentFloor);
        Assert.Equal(1, elevator.DestinationFloor);
        Assert.Equal(Direction.DOWN, elevator.ElevatorDirection);
        Assert.Equal(0, elevator.Passengers.Count);
    }

    /// <summary>
    /// No passengers
    /// queued passengers on 1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_NoPassengers_GetQueued02()
    {
        var floor1 = Building.Floors[1];
        floor1.QueuedPassengers = new List<Passenger>
        {
            new (1, 5),
        };

        var elevator = new Elevator(1);
        elevator.CurrentFloor = 2;
        elevator.DestinationFloor = 1;
        elevator.ElevatorDirection = Direction.DOWN;
        elevator.Passengers = new();

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);
        
        Assert.Equal(1, elevator.CurrentFloor);
        Assert.Equal(5, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.Equal(1, elevator.Passengers.Count);
    }

    /// <summary>
    /// Disembark all passengers
    /// queued passengers on 1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_DisembarksAllPassengers_GetQueuedUP()
    {
        var floor7 = Building.Floors[7];
        floor7.QueuedPassengers = new List<Passenger>
        {
            new (7, 5),
            new (7, 3),
            new (7, 2),
        };

        var elevator = new Elevator(1);
        elevator.CurrentFloor = 2;
        elevator.DestinationFloor = 3;
        elevator.ElevatorDirection = Direction.UP;
        elevator.Passengers = new List<Passenger>
        {
            new(1, 3),
            new(1, 3),
            new(1, 3),
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(7, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.Equal(0, elevator.Passengers.Count);
    }

    /// <summary>
    /// Disembark all passengers
    /// queued passengers on 1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_VIPOnboard()
    {
        var floor = Building.Floors[1];
        floor.QueuedPassengers = new List<Passenger>
        {
            new (1, 5),
        };

        var vip = new Passenger(1, 5);
        vip.VIP = true;

        var elevator = new Elevator(1);
        elevator.CurrentFloor = 2;
        elevator.DestinationFloor = 3;
        elevator.ElevatorDirection = Direction.UP;
        elevator.Passengers = new List<Passenger>
        {
            new(1, 3),
            new(1, 3),
            new(1, 3),
            vip,
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(5, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.Equal(4, elevator.Passengers.Count);
    }

    /// <summary>
    /// Disembark all passengers
    /// queued passengers on 1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_Arrive_Bottom_Floor_No_Queued()
    {
        var elevator = new Elevator(1);
        elevator.CurrentFloor = 2;
        elevator.DestinationFloor = 1;
        elevator.ElevatorDirection = Direction.DOWN;
        elevator.Passengers = new List<Passenger>
        {
            new(10, 1),
            new(10, 1),
            new(10, 1),
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(1, elevator.CurrentFloor);
        Assert.Equal(null, elevator.DestinationFloor);
        Assert.Equal(null, elevator.ElevatorDirection);
        Assert.Equal(0, elevator.Passengers.Count);
    }

    /// <summary>
    /// Disembark all passengers
    /// queued passengers on 1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_Arrive_Bottom_Floor_Queued()
    {
        var floor5 = Building.Floors[5];
        floor5.QueuedPassengers = new List<Passenger>
        {
            new (5, 1),
        };

        var elevator = new Elevator(1);
        elevator.CurrentFloor = 2;
        elevator.DestinationFloor = 1;
        elevator.ElevatorDirection = Direction.DOWN;
        elevator.Passengers = new List<Passenger>
        {
            new(10, 1),
            new(10, 1),
            new(10, 1),
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(1, elevator.CurrentFloor);
        Assert.Equal(5, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.Equal(0, elevator.Passengers.Count);
    }

    /// <summary>
    /// Disembark all passengers
    /// queued passengers on 1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_Arrive_Top_Floor_No_Queued()
    {
        var elevator = new Elevator(1);
        elevator.CurrentFloor = 9;
        elevator.DestinationFloor = 10;
        elevator.ElevatorDirection = Direction.UP;
        elevator.Passengers = new List<Passenger>
        {
            new(1, 10),
            new(1, 10),
            new(1, 10),
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(10, elevator.CurrentFloor);
        Assert.Equal(null, elevator.DestinationFloor);
        Assert.Equal(null, elevator.ElevatorDirection);
        Assert.Equal(0, elevator.Passengers.Count);
    }

    /// <summary>
    /// Disembark all passengers
    /// queued passengers on 1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ElevatorArrivesAtFloor_Arrive_Top_Floor_Queued()
    {
        var floor5 = Building.Floors[5];
        floor5.QueuedPassengers = new List<Passenger>
        {
            new (5, 1),
        };


        var elevator = new Elevator(1);
        elevator.CurrentFloor = 9;
        elevator.DestinationFloor = 10;
        elevator.ElevatorDirection = Direction.UP;
        elevator.Passengers = new List<Passenger>
        {
            new(1, 10),
            new(1, 10),
            new(1, 10),
        };

        Building.Elevators[1] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, elevator.DestinationFloor!.Value);

        Assert.Equal(10, elevator.CurrentFloor);
        Assert.Equal(5, elevator.DestinationFloor);
        Assert.Equal(Direction.DOWN, elevator.ElevatorDirection);
        Assert.Equal(0, elevator.Passengers.Count);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_InactiveFloor_ContinuesTowardDestination()
    {
        Building.Floors[3].IsActive = false;

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 5,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                new(1, 5)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 5);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(5, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.Single(elevator.Passengers);
        Assert.False(elevator.DoorOpen);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_InactiveFloor_DoesNotPickUpQueuedPassengers()
    {
        Building.Floors[3].IsActive = false;

        var queuedPassenger = new Passenger(3, 6);
        Building.Floors[3].QueuedPassengers.Add(queuedPassenger);

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 5,
            ElevatorDirection = Direction.UP
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 5);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Empty(elevator.Passengers);
        Assert.Single(Building.Floors[3].QueuedPassengers);
        Assert.Contains(queuedPassenger, Building.Floors[3].QueuedPassengers);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_NoDropOffOrMatchingQueue_Continues()
    {
        Building.Floors[3].QueuedPassengers.Add(new Passenger(3, 1)); // DOWN

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 6,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                new(1, 6)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 6);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(6, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.Single(elevator.Passengers);
        Assert.Single(Building.Floors[3].QueuedPassengers);
        Assert.False(elevator.DoorOpen);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_DropOffOccurs_OppositeDirectionPassengerRemainsQueued()
    {
        var oppositeDirectionPassenger = new Passenger(3, 1);
        Building.Floors[3].QueuedPassengers.Add(oppositeDirectionPassenger);

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 5,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                new(1, 3),
                new(1, 5)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 5);

        Assert.Equal(3, elevator.CurrentFloor);

        Assert.Single(elevator.Passengers);
        Assert.Equal(5, elevator.Passengers.Single().Destination);

        Assert.Single(Building.Floors[3].QueuedPassengers);
        Assert.Contains(oppositeDirectionPassenger, Building.Floors[3].QueuedPassengers);

        Assert.Equal(5, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.False(elevator.DoorOpen);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_SameDirectionPassengersQueued_PicksThemUp()
    {
        var passenger1 = new Passenger(3, 5);
        var passenger2 = new Passenger(3, 7);

        Building.Floors[3].QueuedPassengers.AddRange(
            new[] { passenger1, passenger2 });

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 6,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                new(1, 6)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 6);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(3, elevator.Passengers.Count);
        Assert.Empty(Building.Floors[3].QueuedPassengers);

        // SetDestination chooses the minimum destination while going up.
        Assert.Equal(5, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_QueuedPassengersExceedCapacity_TakesOnlyAvailableSpaces()
    {
        var existingPassengerCount = Building.MAX_OCCUPANCY - 1;

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 8,
            ElevatorDirection = Direction.UP,
            Passengers = Enumerable.Range(0, existingPassengerCount)
                .Select(_ => new Passenger(1, 8))
                .ToList()
        };

        var queuedPassengers = new List<Passenger>
        {
            new(3, 5),
            new(3, 6),
            new(3, 7)
        };

        Building.Floors[3].QueuedPassengers.AddRange(queuedPassengers);
        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 8);

        Assert.Equal(Building.MAX_OCCUPANCY, elevator.Passengers.Count);
        Assert.Equal(2, Building.Floors[3].QueuedPassengers.Count);

        Assert.DoesNotContain(
            elevator.Passengers,
            p => Building.Floors[3].QueuedPassengers.Any(q => q.Id == p.Id));
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_VIPDestinationReached_VipDisembarks()
    {
        var vip = new Passenger(1, 3)
        {
            VIP = true
        };

        var regularPassenger = new Passenger(1, 6);

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 3,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                vip,
                regularPassenger
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 3);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.DoesNotContain(elevator.Passengers, p => p.Id == vip.Id);
        Assert.Contains(elevator.Passengers, p => p.Id == regularPassenger.Id);
        Assert.Equal(6, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.False(elevator.DoorOpen);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_VIPDestinationReached_VipDisembarks_ReverseCourse()
    {
        var vip = new Passenger(1, 6)
        {
            VIP = true
        };

        var regularPassenger = new Passenger(1, 3);

        var elevator = new Elevator(1)
        {
            CurrentFloor = 5,
            DestinationFloor = 6,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                vip,
                regularPassenger
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 3);

        Assert.Equal(6, elevator.CurrentFloor);
        Assert.DoesNotContain(elevator.Passengers, p => p.Id == vip.Id);
        Assert.Contains(elevator.Passengers, p => p.Id == regularPassenger.Id);
        Assert.Equal(3, elevator.DestinationFloor);
        Assert.Equal(Direction.DOWN, elevator.ElevatorDirection);
        Assert.False(elevator.DoorOpen);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_MultipleVIPsGoingUp_UsesLowestVipDestination()
    {
        var vipAtSeven = new Passenger(1, 7) { VIP = true };
        var vipAtFive = new Passenger(1, 5) { VIP = true };

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 8,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                vipAtSeven,
                vipAtFive,
                new(1, 3)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 8);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(5, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
        Assert.Equal(3, elevator.Passengers.Count);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_MultipleVIPsGoingDown_UsesHighestVipDestination()
    {
        var vipAtTwo = new Passenger(9, 2) { VIP = true };
        var vipAtFour = new Passenger(9, 4) { VIP = true };

        var elevator = new Elevator(1)
        {
            CurrentFloor = 8,
            DestinationFloor = 1,
            ElevatorDirection = Direction.DOWN,
            Passengers = new List<Passenger>
            {
                vipAtTwo,
                vipAtFour,
                new(9, 7)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 1);

        Assert.Equal(7, elevator.CurrentFloor);
        Assert.Equal(4, elevator.DestinationFloor);
        Assert.Equal(Direction.DOWN, elevator.ElevatorDirection);
        Assert.Equal(3, elevator.Passengers.Count);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_EmptyAfterStop_GoingUp_SelectsHighestUpRequest()
    {
        Building.Floors[5].QueuedPassengers.Add(new Passenger(5, 8));
        Building.Floors[7].QueuedPassengers.Add(new Passenger(7, 9));

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 3,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                new(1, 3)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 3);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Empty(elevator.Passengers);
        Assert.Equal(7, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_EmptyAfterStop_GoingDown_SelectsLowestDownRequest()
    {
        Building.Floors[6].QueuedPassengers.Add(new Passenger(6, 2));
        Building.Floors[4].QueuedPassengers.Add(new Passenger(4, 1));

        var elevator = new Elevator(1)
        {
            CurrentFloor = 9,
            DestinationFloor = 8,
            ElevatorDirection = Direction.DOWN,
            Passengers = new List<Passenger>
            {
                new(10, 8)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 8);

        Assert.Equal(8, elevator.CurrentFloor);
        Assert.Empty(elevator.Passengers);
        Assert.Equal(4, elevator.DestinationFloor);
        Assert.Equal(Direction.DOWN, elevator.ElevatorDirection);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_NoSameDirectionRequests_SelectsOppositeDirectionRequest()
    {
        // Previous direction is UP, but all queued passengers want DOWN.
        Building.Floors[6].QueuedPassengers.Add(new Passenger(6, 2));
        Building.Floors[8].QueuedPassengers.Add(new Passenger(8, 1));

        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 3,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                new(1, 3)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 3);

        // For previous UP direction and opposite-direction requests,
        // the implementation selects the minimum source floor.
        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Equal(6, elevator.DestinationFloor);
        Assert.Equal(Direction.UP, elevator.ElevatorDirection);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_DestinationReachedWithNothingToDo_BecomesIdle()
    {
        var elevator = new Elevator(1)
        {
            CurrentFloor = 2,
            DestinationFloor = 3,
            ElevatorDirection = Direction.UP,
            Passengers = new List<Passenger>
            {
                // Passenger is not getting off at floor 3.
                new(1, 6)
            }
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(elevator.Id, 3);

        Assert.Equal(3, elevator.CurrentFloor);
        Assert.Null(elevator.DestinationFloor);
        Assert.Null(elevator.ElevatorDirection);
        Assert.Single(elevator.Passengers);
    }

    [Fact]
    public async Task ElevatorArrivesAtFloor_AboveMaximumFloor_CorrectsToMaximumFloor()
    {
        var elevator = new Elevator(1)
        {
            CurrentFloor = Building.MAX_FLOOR,
            DestinationFloor = Building.MAX_FLOOR,
            ElevatorDirection = Direction.UP,
            Passengers = new()
        };

        Building.Elevators[elevator.Id] = elevator;

        await _service.ElevatorArrivesAtFloor(
            elevator.Id,
            Building.MAX_FLOOR);

        Assert.Equal(Building.MAX_FLOOR, elevator.CurrentFloor);
        Assert.Null(elevator.DestinationFloor);
        Assert.Null(elevator.ElevatorDirection);
    }
}

// Minimal stubs for SignalR hub context
file sealed class MockHub<T> : IHubContext<T> where T : Hub
{
    public IHubClients Clients { get; } = new MockClients();
    public IGroupManager Groups { get; } = new MockGroupManager();
}

file sealed class MockClients : IHubClients
{
    public IClientProxy All { get; } = new MockClientProxy();
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => All;
    public IClientProxy Client(string connectionId) => All;
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => All;
    public IClientProxy Group(string groupName) => All;
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => All;
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => All;
    public IClientProxy User(string userId) => All;
    public IClientProxy Users(IReadOnlyList<string> userIds) => All;
}

file sealed class MockClientProxy : IClientProxy
{
    public Task SendCoreAsync(string method, object?[]? args, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

file sealed class MockGroupManager : IGroupManager
{
    public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}