# dotnetElevators

.NET elevator simulation using `Channel<T>`-based background services.

## Stack

- .NET 10
- `Microsoft.Extensions.Hosting` for DI + background services
- `System.Threading.Channels` for per-elevator async queues

## Architecture

```
PassengerService (spawns passengers on random floors)
       |
       v
BuildingService.CallElevator() -> dispatches best idle elevator
       |
       v
QueueManager (4 x Channel<QueueItem>, one per elevator)
       |
       v
ElevatorManagementService (4 readers, processes arrivals)
       |
       v
BuildingService.ElevatorArrivesAtFloor()
       |
       v
Elevator.ArriveAtFloor() -> AddPassengers / SetDestination
```

## Running

```bash
dotnet run
```

## Config

All in `Building.cs`:

| Constant | Default |
|---|---|
| `MAX_OCCUPANCY` | 12 |
| `MIN_FLOOR` | 1 |
| `MAX_FLOOR` | 10 |
| `ELEVATOR_COUNT` | 4 |
| `ELEVATOR_TRAVEL_SPEED_SEC` | 8 |
| `NEW_PASSENGER_SPAWN_SPEED_SEC` | 15 |
