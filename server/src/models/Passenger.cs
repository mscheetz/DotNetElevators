namespace DotNetElevators;

public class Passenger
{
    public string Id {get;set;}
    public int Source {get;set;}
    public int Destination {get;set;}
    public bool VIP {get;set;}
    public Direction Direction {get;set;}

    public Passenger(int source, int destination)
    {
        Id = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("/", "_")
                    .Replace("+", "-")
                    .TrimEnd('=');

        Source = source;
        Destination = destination;

        Direction = source > destination ? Direction.DOWN : Direction.UP;
    }
}