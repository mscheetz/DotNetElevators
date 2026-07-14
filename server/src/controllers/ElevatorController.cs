using Microsoft.AspNetCore.Mvc;

namespace DotNetElevators;

[Route("api/elevators")]
[ApiController]
public class ElevatorController : ControllerBase
{
    private readonly BuildingService _buildingService;
    private readonly ILogger<ElevatorController> _logger;

    public ElevatorController(
        BuildingService buildingService,
        ILogger<ElevatorController> logger)
    {
        _buildingService = buildingService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetElevators()
    {
        _logger.LogInformation("New request for Elevator status");

        var elevators = Building.Elevators.Values.Select(e => new ElevatorDTO(e));

        return Ok(elevators);
    }

    [HttpGet("{elevatorId:int}")]
    public IActionResult GetElevator(int elevatorId)
    {
        _logger.LogInformation("New request for Elevator {Id} status", elevatorId);

        if (!Building.Elevators.ContainsKey(elevatorId))
        {
            return NotFound();
        }

        var elevator = new ElevatorDTO(Building.Elevators[elevatorId]);

        return Ok(elevator);
    }

    [HttpPut("{elevatorId:int}")]
    public async Task<IActionResult> ToggleElevatorActiveState(int elevatorId)
    {
        _logger.LogInformation("New request to toggle Elevator {Id} Active status", elevatorId);

        var status = await _buildingService.ToggleElevatorStatus(elevatorId);

        return status ? NoContent() : NotFound();
    }

    [HttpPost()]
    public async Task<IActionResult> AddElevator()
    {
        _logger.LogInformation("New request to add Elevator");
        
        var elevatorId = await _buildingService.AddElevator();

        return Ok(elevatorId);
    }
}