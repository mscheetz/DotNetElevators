namespace DotNetElevators.Test;

public record FloorDTO(int FloorNumber, Dictionary<string, int> QueuedPassengerCount, Dictionary<string, int> QueuedVIPCount, int CurrentElevatorCount);