namespace DotNetElevators;

public class NewPassengerRequest
{
    public int Floor {get;set;}
    public int Destination {get;set;}
    public int PassengerCount {get;set;} = 1;

    public List<Passenger> ToPassengers()
    {
        var passengers = new List<Passenger>();

        if (Floor == 0)
        {
            Floor = HelperService.GetRandomFloor();
        }
        if (Destination == 0)
        {
            Destination = HelperService.GetRandomFloor(Floor);
        }

        for (var i = 0; i < PassengerCount; i++)
        {
            passengers.Add(new Passenger(Floor, Destination));
        }

        return passengers;
    }
}