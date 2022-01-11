namespace ExplorerBackend.Services.Queues;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<object, CancellationToken, ValueTask> workItem);
    ValueTask<Func<object, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}