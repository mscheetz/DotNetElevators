using Microsoft.AspNetCore.Mvc;

namespace DotNetElevators;

[Route("api/elevators")]
[ApiController]
public class ElevatorController : ControllerBase
{
    private readonly ILogger<ElevatorController> _logger;

    public ElevatorController(ILogger<ElevatorController> logger)
    {
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
}