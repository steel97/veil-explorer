// Taken from here: https://stackoverflow.com/a/61066995
namespace ExplorerBackend.Core;

public static class AsyncUtils
{
    /// <summary>
    ///     Blocks while condition is true or task is canceled.
    /// </summary>
    /// <param name="ct">
    ///     Cancellation token.
    /// </param>
    /// <param name="condition">
    ///     The condition that will perpetuate the block.
    /// </param>
    /// <param name="pollDelay">
    ///     The delay at which the condition will be polled, in milliseconds.
    /// </param>
    /// <returns>
    ///     <see cref="Task" />.
    /// </returns>
    public static async Task WaitWhileAsync(CancellationToken ct, Func<bool> condition, int pollDelay = 25)
    {
        try
        {
            while (condition())
            {
                await Task.Delay(pollDelay, ct).ConfigureAwait(true);
            }
        }
        catch (TaskCanceledException)
        {
            // ignore: Task.Delay throws this exception when ct.IsCancellationRequested = true
            // In this case, we only want to stop polling and finish this async Task.
        }
    }

    /// <summary>
    ///     Blocks until condition is true or task is canceled.
    /// </summary>
    /// <param name="ct">
    ///     Cancellation token.
    /// </param>
    /// <param name="condition">
    ///     The condition that will perpetuate the block.
    /// </param>
    /// <param name="pollDelay">
    ///     The delay at which the condition will be polled, in milliseconds.
    /// </param>
    /// <returns>
    ///     <see cref="Task" />.
    /// </returns>
    public static async Task WaitUntilAsync(CancellationToken ct, Func<bool> condition, int pollDelay = 25)
    {
        try
        {
            while (!condition())
            {
                await Task.Delay(pollDelay, ct).ConfigureAwait(true);
            }
        }
        catch (TaskCanceledException)
        {
            // ignore: Task.Delay throws this exception when ct.IsCancellationRequested = true
            // In this case, we only want to stop polling and finish this async Task.
        }
    }

    /// <summary>
    ///     Blocks while condition is true or timeout occurs.
    /// </summary>
    /// <param name="ct">
    ///     The cancellation token.
    /// </param>
    /// <param name="condition">
    ///     The condition that will perpetuate the block.
    /// </param>
    /// <param name="pollDelay">
    ///     The delay at which the condition will be polled, in milliseconds.
    /// </param>
    /// <param name="timeout">
    ///     Timeout in milliseconds.
    /// </param>
    /// <exception cref="TimeoutException">
    ///     Thrown after timeout milliseconds
    /// </exception>
    /// <returns>
    ///     <see cref="Task" />.
    /// </returns>
    public static async Task WaitWhileAsync(CancellationToken ct, Func<bool> condition, int pollDelay, int timeout)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Task waitTask = WaitWhileAsync(cts.Token, condition, pollDelay);
        Task timeoutTask = Task.Delay(timeout, cts.Token);
        Task finishedTask = await Task.WhenAny(waitTask, timeoutTask).ConfigureAwait(true);

        if (!ct.IsCancellationRequested)
        {
            cts.Cancel();                            // Cancel unfinished task
            await finishedTask.ConfigureAwait(true); // Propagate exceptions
            if (finishedTask == timeoutTask)
            {
                throw new TimeoutException();
            }
        }
    }

    /// <summary>
    ///     Blocks until condition is true or timeout occurs.
    /// </summary>
    /// <param name="ct">
    ///     Cancellation token
    /// </param>
    /// <param name="condition">
    ///     The condition that will perpetuate the block.
    /// </param>
    /// <param name="pollDelay">
    ///     The delay at which the condition will be polled, in milliseconds.
    /// </param>
    /// <param name="timeout">
    ///     Timeout in milliseconds.
    /// </param>
    /// <exception cref="TimeoutException">
    ///     Thrown after timeout milliseconds
    /// </exception>
    /// <returns>
    ///     <see cref="Task" />.
    /// </returns>
    public static async Task WaitUntilAsync(CancellationToken ct, Func<bool> condition, int pollDelay, int timeout)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Task waitTask = WaitUntilAsync(cts.Token, condition, pollDelay);
        Task timeoutTask = Task.Delay(timeout, cts.Token);
        Task finishedTask = await Task.WhenAny(waitTask, timeoutTask).ConfigureAwait(true);

        if (!ct.IsCancellationRequested)
        {
            cts.Cancel();                            // Cancel unfinished task
            await finishedTask.ConfigureAwait(true); // Propagate exceptions
            if (finishedTask == timeoutTask)
            {
                throw new TimeoutException();
            }
        }
    }
}