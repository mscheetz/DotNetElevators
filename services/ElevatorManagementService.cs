using System.Threading.Channels;

namespace DotNetElevators;

public class ElevatorManagementService: BackgroundService
{
    private readonly BuildingService _buildingService;
    private readonly QueueManager _queues;
    private readonly ILogger<ElevatorManagementService> _logger;

    public ElevatorManagementService(
        BuildingService buildingService,
        QueueManager queueManager, 
        ILogger<ElevatorManagementService> logger)
    {
        _buildingService = buildingService;
        _queues = queueManager;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task queue1Task = ProcessQueueAsync(
            "Elevator 1",
            _queues.Queue1.Reader,
            ProcessElevator,
            stoppingToken);

        Task queue2Task = ProcessQueueAsync(
            "Elevator 2",
            _queues.Queue2.Reader,
            ProcessElevator,
            stoppingToken);

        Task queue3Task = ProcessQueueAsync(
            "Elevator 3",
            _queues.Queue3.Reader,
            ProcessElevator,
            stoppingToken);

        Task queue4Task = ProcessQueueAsync(
            "Elevator 4",
            _queues.Queue4.Reader,
            ProcessElevator,
            stoppingToken);

        return Task.WhenAll(
            queue1Task,
            queue2Task,
            queue3Task,
            queue4Task);
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