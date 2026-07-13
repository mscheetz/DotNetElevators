namespace DotNetElevators;

public static class HelperService
{
    public static int GetRandomFloor() => Random.Shared.Next(Building.MIN_FLOOR, Building.MAX_FLOOR + 1);

    public static int GetRandomFloor(int excludedValue)
    {
        int floor = 0;
        while (true)
        {
            floor = HelperService.GetRandomFloor();

            if (floor != excludedValue)
            {
                break;
            }
        }

        return floor;
    }

    public static bool GetRandomizedVIP()
    {
        return Random.Shared.NextDouble() < Building.VIP_PROBABILITY;
    }
}