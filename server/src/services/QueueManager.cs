using System.Collections.Concurrent;
using System.Threading.Channels;

namespace DotNetElevators;

public sealed class QueueManager
{
    private readonly ConcurrentDictionary<int, Channel<QueueItem>> _queues = [];
    private readonly int _capacity;

    private readonly Channel<QueueRegistration> _newQueues = Channel.CreateUnbounded<QueueRegistration>();

    public ChannelReader<QueueRegistration> NewQueues => _newQueues.Reader;

    public QueueManager(int capacity = 1000)
    {
        if (capacity <= 0)
        {
            capacity = 1;
        }

        _capacity = capacity;
    }

    public Channel<QueueItem> GetQueue(int elevatorNumber)
    {
        if (elevatorNumber <= 0)
        {
            elevatorNumber = 1;
        }

        return _queues.GetOrAdd(elevatorNumber,
        number =>
        {
            var queue = CreateQueue();

            if (!_newQueues.Writer.TryWrite(new QueueRegistration(number, queue)))
            {
                throw new Exception($"Could not register queue for elevator {number}");
            }

            return queue;
        });
    }

    private Channel<QueueItem> CreateQueue()
    {
        return Channel.CreateBounded<QueueItem>(
            new BoundedChannelOptions(_capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
    }

    public async ValueTask AddToQueueAsync(QueueItem item, CancellationToken cancellationToken = default)
    {
        var queue = GetQueue(item.elevatorNumber);

        await queue.Writer.WriteAsync(item, cancellationToken);
    }
}

public sealed record QueueRegistration(
    int ElevatorNumber,
    Channel<QueueItem> Queue);