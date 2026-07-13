using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/hubs/building")
    .WithAutomaticReconnect()
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Debug);
    })
    .Build();

connection.On<DotNetElevators.Test.ElevatorDTO>("ElevatorUpdated", elevator =>
{
    Console.WriteLine($"Elevator {elevator.Id} : on floor {elevator.CurrentFloor} : direction {elevator.ElevatorDirection} : Has VIP: {elevator.HasVIPs}");
});

connection.On<DotNetElevators.Test.FloorDTO>("FloorUpdated", floor =>
{
    Console.WriteLine($"Floor updated: {floor.FloorNumber} : queuedPassengers {floor.QueuedPassengerCount.Values.Sum()} : queued VIPs {floor.QueuedVIPCount.Values.Sum()}");
});

connection.On<DotNetElevators.Test.PassengerDTO>("PassengerUpdated", passenger =>
{
    Console.WriteLine($"Passenger updated: {passenger.Id} : on floor {passenger.FloorNumber}");
});

connection.Reconnecting += error =>
{
    Console.WriteLine($"Reconnecting: {error?.Message}");
    return Task.CompletedTask;
};

connection.Reconnected += connectionId =>
{
    Console.WriteLine($"Reconnected: {connectionId}");
    return Task.CompletedTask;
};

connection.Closed += error =>
{
    Console.WriteLine($"Disconnected: {error?.Message}");
    
    return Task.CompletedTask;
};

try {
    await connection.StartAsync();

    Console.WriteLine("Connected to SignalR hub.");
    Console.WriteLine("Press Enter to exit.");

    Console.ReadLine();
    await connection.StopAsync();
}
catch (Exception ex)
{
    Console.WriteLine("Connection failed", ex);
}
finally
{
    await connection.DisposeAsync();
}

