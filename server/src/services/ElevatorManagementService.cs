using System.Threading.Channels;

namespace DotNetElevators;

public class ElevatorManagementService: BackgroundService
{
    private readonly BuildingService _buildingService;
    private readonly QueueManager _queueManager;
    private readonly ILogger<ElevatorManagementService> _logger;

    public ElevatorManagementService(
        BuildingService buildingService,
        QueueManager queueManager, 
        ILogger<ElevatorManagementService> logger)
    {
        _buildingService = buildingService;
        _queueManager = queueManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueTasks = new List<Task>();

        try
        {
            await foreach (var registration in _queueManager.NewQueues.ReadAllAsync(stoppingToken))
            {
                var task = ProcessQueueAsync(
                    $"Elevator {registration.ElevatorNumber}",
                    registration.Queue.Reader,
                    ProcessElevator,
                    stoppingToken
                );

                queueTasks.Add(task);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {            
        }
        
        await Task.WhenAll(queueTasks);
    }

    private async Task ProcessQueueAsync(
        string queueName, 
        ChannelReader<QueueItem> reader,
        Func<QueueItem, CancellationToken, Task> procesItem, 
        CancellationToken token)
    {
        try
        {
            await foreach (QueueItem item in reader.ReadAllAsync(token))
            {
                try
                {
                    _logger.LogInformation($"[{item.elevatorNumber}] Elevator {item.elevatorNumber} Has Arrived");

                    await procesItem(item, token);
                }
                catch (OperationCanceledException)
                    when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thronw");
                }
            }
        }
        catch (OperationCanceledException)
                    when (token.IsCancellationRequested)
        {
            
        }
    }

    private async Task ProcessElevator(QueueItem queueItem, CancellationToken token)
    {
        var delay = Building.ELEVATOR_TRAVEL_SPEED_SEC * 1000;

        await Task.Delay(delay, token);

        await _buildingService.ElevatorArrivesAtFloor(queueItem.elevatorNumber, queueItem.destinationFloor);
    }
}