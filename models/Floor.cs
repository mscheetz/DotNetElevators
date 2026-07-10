
namespace DotNetElevators;

public class Floor 
{
    public int FloorNumber { get;set; }
    public List<Passenger> QueuedPassengers {get;set;} = new();

    public Floor(int floorNumber)
    {
        FloorNumber = floorNumber;
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