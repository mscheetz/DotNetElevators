
namespace DotNetElevators;

public class Elevator
{
    public int Id {get;set;}
    public int MaxOccupancy {get;set;}
    public int CurrentFloor {get;set;}
    public int? DestinationFloor {get;set;}
    public List<Passenger> Passengers {get;set;} = new();
    public Direction? ElevatorDirection = null;
    public bool DoorOpen {get;set;}

    public Elevator(int id)
    {
        Id = id;
        MaxOccupancy = Building.MAX_OCCUPANCY;
        CurrentFloor = Building.MIN_FLOOR;
        DestinationFloor = null;
        Passengers = new();
        ElevatorDirection = null;
        DoorOpen = false;
    }

    public HashSet<string> AddPassengers(List<Passenger> pendingPasengers) 
    {
        var occupancy = Passengers.Count;
        var pending_count = pendingPasengers.Count;

        if (occupancy == MaxOccupancy)
        {
            return new();
        }

        var pending_occupancy_count = occupancy + pending_count;
        var new_passenger_ids = new HashSet<string>();

        if (pending_occupancy_count <= MaxOccupancy) 
        {
            new_passenger_ids = pendingPasengers.Select(p => p.Id).ToHashSet();
        }
        else if (pending_occupancy_count > MaxOccupancy)
        {
            var eligible_count = MaxOccupancy - occupancy;
            new_passenger_ids = pendingPasengers.Take(eligible_count).Select(p => p.Id).ToHashSet();
        }

        Passengers.AddRange(pendingPasengers.Where(p => new_passenger_ids.Contains(p.Id)));

        return new_passenger_ids;
    }

    public void SetDestination()
    {
        if (!Passengers.Any())
        {
            DestinationFloor = null;
            ElevatorDirection = null;
            return;
        }

        var vips = Passengers.Where(p => p.VIP);

        if (ElevatorDirection == Direction.UP)
        {            
            DestinationFloor = vips.Any() 
                                ? vips.Min(p => p.Destination)
                                : Passengers.Min(p => p.Destination);
        }
        else 
        {
            DestinationFloor = vips.Any()
                                ? vips.Max(p => p.Destination)
                                : Passengers.Max(p => p.Destination);
        }
    }

    public HashSet<string> ArriveAtFloor(int newFloor, List<Passenger> pendingPassengers)
    {
        Console.WriteLine($"[{Id}] Arriving at floor {newFloor}");
        CurrentFloor = newFloor;

        if (newFloor == Building.MAX_FLOOR)
        {
            ElevatorDirection = Direction.DOWN;            
        }
        else if (newFloor == Building.MIN_FLOOR) 
        {
            ElevatorDirection = Direction.UP;
        }

        if (DestinationFloor == newFloor || pendingPassengers.Any())
        {
            if (DestinationFloor == newFloor) 
            {
                Console.WriteLine($"[{Id}] Passengers ready to disembark");
            }
            if (pendingPassengers.Any())
            {
                Console.WriteLine($"[{Id}] Pending passengers ({pendingPassengers.Count}) queued");
            }

            OpenDoors();

            DirectionChangedCheck();

            var newPassengerIds = AddPassengers(pendingPassengers.Where(p => p.Direction == ElevatorDirection).ToList());

            SetDestination();

            DoorOpen = false;

            return newPassengerIds;
        }
        else 
        {
            Console.WriteLine($"[{Id}] No passengers, continuing");
        }

        return new();
    }

    public void OpenDoors()
    {
        DoorOpen = true;
        Passengers.RemoveAll(p => p.Destination == CurrentFloor);
    }

    public bool DirectionChangedCheck()
    {
        if (!Passengers.Any())
        {
            return false;
        }

        var minFloor = Passengers.Min(p => p.Destination);
        var maxFloor = Passengers.Max(p => p.Destination);

        if (ElevatorDirection == Direction.UP 
            && maxFloor < CurrentFloor)
        {
            ElevatorDirection = Direction.DOWN;
            Console.WriteLine($"[{Id}] Direction changed continuing");
            return true;
        }

        if (ElevatorDirection == Direction.DOWN
            && minFloor > CurrentFloor)
        {
            ElevatorDirection = Direction.UP;
            Console.WriteLine($"[{Id}] Direction changed continuing");
            return true;   
        }

        return false;
    }
}