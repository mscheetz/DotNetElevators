using System.Threading.Channels;

namespace DotNetElevators;

public sealed class QueueManager
{
    public Channel<QueueItem> Queue1 { get; } = CreateQueue();
    public Channel<QueueItem> Queue2 { get; } = CreateQueue();
    public Channel<QueueItem> Queue3 { get; } = CreateQueue();
    public Channel<QueueItem> Queue4 { get; } = CreateQueue();

    private static Channel<QueueItem> CreateQueue()
    {
        return Channel.CreateBounded<QueueItem>(
            new BoundedChannelOptions(capacity: 1_000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
    }

    public async ValueTask AddToQueueAsync(QueueItem item, CancellationToken cancellationToken = default)
    {
        switch (item.elevatorNumber)
        {
            case 1:
                await AddToQueue1Async(item, cancellationToken);
                break;

            case 2:
                await AddToQueue2Async(item, cancellationToken);
                break;

            case 3:
                await AddToQueue3Async(item, cancellationToken);
                break;

            case 4:
                await AddToQueue4Async(item, cancellationToken);
                break;

            default:                
                break;

        }
    }

    public ValueTask AddToQueue1Async(
        QueueItem item,
        CancellationToken cancellationToken = default)
    {
        return Queue1.Writer.WriteAsync(item, cancellationToken);
    }

    public ValueTask AddToQueue2Async(
        QueueItem item,
        CancellationToken cancellationToken = default)
    {
        return Queue2.Writer.WriteAsync(item, cancellationToken);
    }

    public ValueTask AddToQueue3Async(
        QueueItem item,
        CancellationToken cancellationToken = default)
    {
        return Queue3.Writer.WriteAsync(item, cancellationToken);
    }

    public ValueTask AddToQueue4Async(
        QueueItem item,
        CancellationToken cancellationToken = default)
    {
        return Queue4.Writer.WriteAsync(item, cancellationToken);
    }
}