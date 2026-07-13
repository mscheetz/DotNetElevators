namespace DotNetElevators;

public static class Building
{
    public static Dictionary<int, Floor> Floors {get;set;} = [];
    public static Dictionary<int, Elevator> Elevators {get;set;} = [];

    public static int MAX_OCCUPANCY = 12;
    public static int MIN_FLOOR = 1;
    public static int MAX_FLOOR = 10;
    public static int ELEVATOR_COUNT = 4;
    public static int ELEVATOR_TRAVEL_SPEED_SEC = 2;
    public static int NEW_PASSENGER_SPAWN_SPEED_SEC = 3;
    public static double VIP_PROBABILITY = 0.10;
}