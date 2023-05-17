namespace TickTockTeatime;

/// <summary>
/// Manages a timer system that ticks at a defined interval.
/// </summary>
public class TickSystem : IDisposable
{
    public enum ErrorHandlingPolicy
    {
        Ignore,
        LogAndContinue,
        Stop
    }

    private readonly TimeSpan _tickInterval;
    private readonly Timer _timer;
    private bool _isDisposed;
    
    /// <summary>
    /// The error handling policy for the TickSystem.
    /// </summary>
    public ErrorHandlingPolicy Policy { get; set; } = ErrorHandlingPolicy.Ignore;
    
    /// <summary>
    /// Event triggered when an error occurs during the process.
    /// </summary>
    public delegate void ErrorHandler(Exception ex);
    public event ErrorHandler? OnError;
    
    /// <summary>
    /// Event triggered when the timer performs a tick.
    /// </summary>
    public delegate void TickHandler();
    public event TickHandler? OnTick;

    /// <summary>
    /// Initializes a new instance of the TickSystem class.
    /// </summary>
    /// <param name="tickInterval">The time interval between each tick.</param>
    public TickSystem(TimeSpan tickInterval)
    {
        _tickInterval = tickInterval;
        _timer = new Timer(Tick, null, Timeout.InfiniteTimeSpan, tickInterval);
    }

    /// <summary>
    /// Releases all resources used by the TickSystem instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Starts the timer, initiating a tick at the defined interval.
    /// </summary>
    public void Start()
    {
        _timer.Change(TimeSpan.Zero, _tickInterval);
    }

    /// <summary>
    /// Stops the timer, pausing the tick operations.
    /// </summary>
    public void Stop()
    {
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Invokes the OnTick event when the timer performs a tick.
    /// </summary>
    private void Tick(object? state)
    {
        try
        {
            OnTick?.Invoke();
        }
        catch (Exception ex)
        {
            switch (Policy)
            {
                case ErrorHandlingPolicy.Ignore:
                    break;
                case ErrorHandlingPolicy.LogAndContinue:
                    OnError?.Invoke(ex);
                    break;
                case ErrorHandlingPolicy.Stop:
                    OnError?.Invoke(ex);
                    Stop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Releases the timer resource.
    /// </summary>
    /// <param name="disposing">
    /// Indicates whether the method call comes from a Dispose method (its value is true) or from a
    /// finalizer (its value is false).
    /// </param>
    private void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        if (disposing) _timer.Dispose();
        _isDisposed = true;
    }
}