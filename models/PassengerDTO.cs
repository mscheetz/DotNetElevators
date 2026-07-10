namespace DotNetElevators;

public class PassengerDTO
{
    public string Id {get;set;}
    public string Status {get;set;} = string.Empty;
    public int? ElevatorId {get;set;}
    public int? FloorNumber {get;set;}
    public string Direction {get;set;} = string.Empty;

    public PassengerDTO(string id, Floor? floor, Elevator? elevator)
    {
        Id = id;
        if (floor is not null)
        {
            Status = "Waiting";
            FloorNumber = floor.FloorNumber;
            Direction = floor.QueuedPassengers.First(p => p.Id == id).Direction.ToString();
        }
        else if (elevator is not null)
        {
            Status = $"Going {elevator.ElevatorDirection.ToString()}";
            ElevatorId = elevator.Id;
            Direction = elevator.ElevatorDirection.HasValue ? elevator.ElevatorDirection.Value.ToString() : "Idle";
        }
    }
}