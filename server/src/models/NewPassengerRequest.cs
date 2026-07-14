namespace DotNetElevators;

public class NewPassengerRequest
{
    public int Floor {get;set;}
    public int Destination {get;set;}
    public int PassengerCount {get;set;} = 1;
    public bool Vip {get;set;}
    public bool RandomizeVip {get;set;}

    public List<Passenger> ToPassengers(HashSet<int> invalidFloors)
    {
        var passengers = new List<Passenger>();

        for (var i = 0; i < PassengerCount; i++)
        {
            if (Floor == 0)
            {
                Floor = HelperService.GetRandomFloor(invalidFloors);
            }
            if (Destination == 0)
            {
                invalidFloors.Add(Floor);
                Destination = HelperService.GetRandomFloor(invalidFloors);
            }
            
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