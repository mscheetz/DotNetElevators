namespace DotNetElevators;

public class FloorDTO
{
    public int FloorNumber {get;set;}
    public Dictionary<string, int> QueuedPassengerCount {get;set;}
    public int CurrentElevatorCount {get;set;}

    public FloorDTO(Floor floor, IEnumerable<Elevator> elevators)
    {
        FloorNumber = floor.FloorNumber;
        QueuedPassengerCount = new()
        {
            { Direction.DOWN.ToString(), floor.QueuedPassengers.Count(p => p.Direction == Direction.DOWN) },
            { Direction.UP.ToString(), floor.QueuedPassengers.Count(p => p.Direction == Direction.UP) }
        };
        CurrentElevatorCount = elevators.Count(e => e.CurrentFloor == floor.FloorNumber);
    }
}