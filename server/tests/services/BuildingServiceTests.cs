using dotnetElevators;

namespace dotnetElevators.Tests;

public class BuildingServiceTests
{
    [Fact]
    public void ElevatorArrivesAtFloor()
    {
        var floor = 3;
        var elevator = new Elevator(1);
        elevator.Passengers = new List<Passenger>
        {
            new Passenger()
        };
    }
}
