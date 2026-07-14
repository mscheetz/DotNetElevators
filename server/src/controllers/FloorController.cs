using Microsoft.AspNetCore.Mvc;

namespace DotNetElevators;

[Route("api/floors")]
[ApiController]
public class FloorController : ControllerBase
{
    private readonly BuildingService _buildingService;
    private readonly ILogger<FloorController> _logger;

    public FloorController(
        BuildingService buildingService,
        ILogger<FloorController> logger)
    {
        _buildingService = buildingService;
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

    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactiveFloors()
    {
        _logger.LogInformation("New request for inactive floors");

        var inactiveFloors = Building.Floors.Values.Where(f => !f.IsActive).Select(f => f.FloorNumber).ToHashSet();

        return Ok(inactiveFloors);
    }

    [HttpPost()]
    public async Task<IActionResult> AddFloor()
    {
        var floorNumber = await _buildingService.AddFloor();

        return Ok(floorNumber);
    }

    [HttpPut("{floorNumber:int}")]
    public async Task<IActionResult> ToggleFloorStatus(int floorNumber)
    {
        var status = await _buildingService.ToggleFloorStatus(floorNumber);

        return Ok(status);
    }
}