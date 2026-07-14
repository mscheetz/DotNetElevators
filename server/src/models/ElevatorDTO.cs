namespace DotNetElevators;

public class ElevatorDTO
{
    public int Id {get;set;}
    public int CurrentFloor {get;set;}
    public int? DestinationFloor { get;set; }
    public string ElevatorDirection {get;set;}
    public int PassengerCount {get;set;}
    public bool HasVIPs {get;set;}
    public bool IsActive {get;set;}

    public ElevatorDTO(Elevator elevator)
    {
        Id = elevator.Id;
        CurrentFloor = elevator.CurrentFloor;
        DestinationFloor = elevator.DestinationFloor;
        ElevatorDirection = elevator.ElevatorDirection.HasValue ? elevator.ElevatorDirection.Value.ToString() : "Idle";
        PassengerCount = elevator.Passengers.Count;
        HasVIPs = elevator.Passengers.Any(p => p.VIP);
        IsActive = !elevator.IsActive;
    }

}