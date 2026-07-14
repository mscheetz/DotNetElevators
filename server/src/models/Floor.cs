
namespace DotNetElevators;

public class Floor 
{
    public int FloorNumber { get;set; }
    public List<Passenger> QueuedPassengers {get;set;} = new();
    public bool IsActive {get;set;}

    public Floor(int floorNumber)
    {
        FloorNumber = floorNumber;
        IsActive = true;
    }

    public void QueuePassenger(Passenger passenger)
    {
        QueuedPassengers.Add(passenger);
    }

    public void PassengersDeparted(HashSet<string> passengerIds)
    {
        if (passengerIds.Any())
        {
            QueuedPassengers.RemoveAll(p => passengerIds.Contains(p.Id));
        }
    }
}