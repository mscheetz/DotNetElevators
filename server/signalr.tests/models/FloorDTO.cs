namespace DotNetElevators.SignalR.Test;

public record FloorDTO(int FloorNumber, Dictionary<string, int> QueuedPassengerCount, Dictionary<string, int> QueuedVIPCount, int CurrentElevatorCount);