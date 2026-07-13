namespace DotNetElevators;

public class FloorDTO
{
    public int FloorNumber {get;set;}
    public Dictionary<string, int> QueuedPassengerCount {get;set;}
    public Dictionary<string, int> QueuedVIPCount {get;set;}
    public int CurrentElevatorCount {get;set;}

    public FloorDTO(Floor floor, IEnumerable<Elevator> elevators)
    {
        FloorNumber = floor.FloorNumber;
        QueuedPassengerCount = new()
        {
            { Direction.DOWN.ToString(), floor.QueuedPassengers.Count(p => p.Direction == Direction.DOWN) },
            { Direction.UP.ToString(), floor.QueuedPassengers.Count(p => p.Direction == Direction.UP) }
        };
        QueuedVIPCount = new()
        {
            { Direction.DOWN.ToString(), floor.QueuedPassengers.Count(p => p.Direction == Direction.DOWN && p.VIP) },
            { Direction.UP.ToString(), floor.QueuedPassengers.Count(p => p.Direction == Direction.UP && p.VIP) }
        };
        CurrentElevatorCount = elevators.Count(e => e.CurrentFloor == floor.FloorNumber);
    }
}