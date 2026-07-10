using Microsoft.AspNetCore.Mvc;

namespace DotNetElevators;

[Route("api/passengers")]
[ApiController]
public class PassengerController : ControllerBase
{
    private readonly PassengerService _passengerService;
    private readonly ILogger<PassengerController> _logger;

    public PassengerController(
        PassengerService passengerService,
        ILogger<PassengerController> logger)
    {
        _passengerService = passengerService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetPassengers()
    {
        _logger.LogInformation("New request for Passenger status");

        var passengerIds = GetPassengerIds();

        var passengers = new List<PassengerDTO>();

        var elevatorLookup = Building.Elevators.Values
            .SelectMany(e => e.Passengers.Select(p => new {p.Id, Elevator = e}))
            .ToDictionary(k => k.Id, v => v.Elevator);

        var floorLookup = Building.Floors.Values
            .SelectMany(f => f.QueuedPassengers.Select(p => new {p.Id, Floor = f}))
            .ToDictionary(k => k.Id, v => v.Floor);

        foreach (var id in passengerIds)
        {
            passengers.Add(new PassengerDTO(id, floorLookup.GetValueOrDefault(id), elevatorLookup.GetValueOrDefault(id)));
        }

        return Ok(passengers);
    }

    [HttpGet("{passengerId}")]
    public IActionResult GetPassenger(string passengerId)
    {
        _logger.LogInformation("New request for Passenger status");

        var passengerIds = GetPassengerIds();

        if (!passengerIds.TryGetValue(passengerId, out _))
        {
            return NotFound();
        }

        var elevatorLookup = Building.Elevators.Values
            .SelectMany(e => e.Passengers.Select(p => new {p.Id, Elevator = e}))
            .ToDictionary(k => k.Id, v => v.Elevator);

        var floorLookup = Building.Floors.Values
            .SelectMany(f => f.QueuedPassengers.Select(p => new {p.Id, Floor = f}))
            .ToDictionary(k => k.Id, v => v.Floor);

        var passenger = new PassengerDTO(passengerId, floorLookup.GetValueOrDefault(passengerId), elevatorLookup.GetValueOrDefault(passengerId));

        return Ok(passenger);
    }

    [HttpPost]
    public async Task<IActionResult> AddPassengers([FromBody]NewPassengerRequest incoming)
    {
        if (incoming.Floor < 0 || incoming.Floor > Building.MAX_FLOOR)
        {
            return BadRequest("Invalid Floor");
        }
        if (incoming.Destination < 0 || incoming.Destination > Building.MAX_FLOOR)
        {
            return BadRequest("Invalid Destination");
        }
        if (incoming.PassengerCount <= 0)
        {
            incoming.PassengerCount = 1;
        }

        var newPassengers = incoming.ToPassengers();

        foreach (var passenger in newPassengers)
        {
            await _passengerService.AddNewPassenger(passenger);
        }

        return NoContent();
    }

    private HashSet<string> GetPassengerIds()
    {        
        var elevatorPassengerIds = Building.Elevators.Values.SelectMany(e => e.Passengers.Select(p => p.Id)).ToHashSet();
        var floorPassengerIds = Building.Floors.Values.SelectMany(f => f.QueuedPassengers.Select(p => p.Id)).ToHashSet();

        var passengerIds = elevatorPassengerIds
                            .Union(floorPassengerIds)
                            .ToHashSet();

        return passengerIds;
    }
}