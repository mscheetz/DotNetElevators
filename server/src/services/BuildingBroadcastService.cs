using Microsoft.AspNetCore.SignalR;

namespace DotNetElevators;

public sealed class BuildingBroadcastService
{
    private readonly IHubContext<BuildingHub> _hub;

    public BuildingBroadcastService(IHubContext<BuildingHub> hub)
    {
        _hub = hub;
    }

    public Task BroadcastElevator(ElevatorDTO elevator, CancellationToken cancellationToken = default)
    {
        return _hub.Clients.All.SendAsync("ElevatorUpdated", elevator, cancellationToken);
    }

    public Task BroadcastFloor(int floorNumber, CancellationToken cancellationToken = default)
    {
        var floor = Building.Floors[floorNumber];
        var floorDto = new FloorDTO(floor, Building.Elevators.Values);

        return BroadcastFloor(floorDto, cancellationToken);
    }

    public Task BroadcastFloor(FloorDTO floor, CancellationToken cancellationToken = default)
    {
        return _hub.Clients.All.SendAsync("FloorUpdated", floor, cancellationToken);
    }

    public Task BroadcastPassenger(string passengerId, CancellationToken cancellationToken = default)
    {
        var elevatorLookup = Building.Elevators.Values
            .SelectMany(e => e.Passengers.Select(p => new {p.Id, Elevator = e}))
            .ToDictionary(k => k.Id, v => v.Elevator);

        var floorLookup = Building.Floors.Values
            .SelectMany(f => f.QueuedPassengers.Select(p => new {p.Id, Floor = f}))
            .ToDictionary(k => k.Id, v => v.Floor);

        var passenger = new PassengerDTO(passengerId, floorLookup.GetValueOrDefault(passengerId), elevatorLookup.GetValueOrDefault(passengerId));

        return BroadcastPassenger(passenger, cancellationToken);
    }

    public Task BroadcastPassenger(PassengerDTO passenger, CancellationToken cancellationToken = default)
    {
        return _hub.Clients.All.SendAsync("PassengerUpdated", passenger, cancellationToken);
    }
}