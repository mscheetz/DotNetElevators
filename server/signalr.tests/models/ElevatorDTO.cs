namespace DotNetElevators.SignalR.Test;

public record ElevatorDTO(int Id, int CurrentFloor, int? DestinationFloor, string ElevatorDirection, int PassengerCount, bool HasVIPs);