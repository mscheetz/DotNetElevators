using Microsoft.AspNetCore.Mvc;

namespace DotNetElevators;

[Route("api/floors")]
[ApiController]
public class FloorController : ControllerBase
{
    private readonly ILogger<FloorController> _logger;

    public FloorController(ILogger<FloorController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetFloors()
    {
        _logger.LogInformation("New request for Floor status");

        var floors = Building.Floors.Select(f => new FloorDTO(f.Value, Building.Elevators.Values));

        return Ok(floors);
    }

    [HttpGet("{floorNumber:int}")]
    public IActionResult GetFloor(int floorNumber)
    {
        _logger.LogInformation("New request for Floor {FloorNumber} status", floorNumber);

        if (!Building.Floors.ContainsKey(floorNumber))
        {
            return NotFound();
        }

        var floor = new FloorDTO(Building.Floors[floorNumber], Building.Elevators.Values);

        return Ok(floor);
    }
}