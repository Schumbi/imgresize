namespace ImageResizer;

public static class Concurrent
{
    /// <summary>
    /// Runs a synchronous function in a new task.
    /// Uses a semaphore to limit the maximum number of concurrent tasks.
    /// </summary>
    /// <typeparam name="T">Return type of function.</typeparam>
    /// <param name="function">Function to execute.</param>
    /// <param name="concurrencyLimit">Semaphore.</param>
    /// <returns>Task with the return value of the function.</returns>
    public static async Task<T> Run<T>(Func<T> function, SemaphoreSlim concurrencyLimit)
    {
        await concurrencyLimit.WaitAsync();
        try
        {
            T result = await Task.Run(function);
            return result;
        }
        finally
        {
            concurrencyLimit.Release();
        }
    }
}
