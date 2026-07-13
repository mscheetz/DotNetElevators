namespace DotNetElevators;

public class NewPassengerRequest
{
    public int Floor {get;set;}
    public int Destination {get;set;}
    public int PassengerCount {get;set;} = 1;
    public bool Vip {get;set;}
    public bool RandomizeVip {get;set;}

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
            var passenger = new Passenger(Floor, Destination);
            
            if (Vip)
            {
                passenger.VIP = true;
            }
            else if (RandomizeVip)
            {
                passenger.VIP = HelperService.GetRandomizedVIP();
            }

            passengers.Add(passenger);
        }

        return passengers;
    }
}