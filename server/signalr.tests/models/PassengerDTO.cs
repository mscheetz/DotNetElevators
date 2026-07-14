namespace DotNetElevators.SignalR.Test;

public record PassengerDTO(string Id, string Status, int? ElevatorId, int? FloorNumber, bool VIP, string Direction);