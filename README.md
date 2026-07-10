# dotnetElevators

Office building elevator simulator using `Channel<T>`-based background services with an ASP.NET Web API.

## Stack

- .NET 10
- ASP.NET Core (`Microsoft.NET.Sdk.Web`)
- `System.Threading.Channels` for per-elevator async queues

## Architecture

```
PassengerTimer (timed background loop)
       |
       v
PassengerService.AddNewPassenger()
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

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/floors` | All floors with queued passenger counts by direction |
| `GET` | `/api/floors/{floorNumber}` | Single floor status |
| `GET` | `/api/elevators` | All elevators (current floor, direction, occupancy, destination) |
| `GET` | `/api/elevators/{id}` | Single elevator state |
| `GET` | `/api/passengers` | All passengers (waiting on floors or riding elevators) |
| `GET` | `/api/passengers/{id}` | Single passenger status |
| `POST` | `/api/passengers` | Spawn one or more passengers |

### POST /api/passengers

```json
{
  "floor": 3,
  "destination": 8,
  "passengerCount": 2
}
```

Fields are optional. `0` or omitted values pick random floor/destination.

## Running

```bash
dotnet run --project server/src/dotnetElevators.csproj 
```

Opens `http://localhost:5000` by default. Simulation runs immediately; API is available on startup.

## Config

All in `Building.cs`:

| Constant | Default |
|---|---|
| `MAX_OCCUPANCY` | 12 |
| `MIN_FLOOR` | 1 |
| `MAX_FLOOR` | 10 |
| `ELEVATOR_COUNT` | 4 |
| `ELEVATOR_TRAVEL_SPEED_SEC` | 2 |
| `NEW_PASSENGER_SPAWN_SPEED_SEC` | 3 |