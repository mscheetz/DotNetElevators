namespace DotNetElevators.Test;

public record PassengerDTO(string Id, string Status, int? ElevatorId, int? FloorNumber, bool VIP, string Direction);