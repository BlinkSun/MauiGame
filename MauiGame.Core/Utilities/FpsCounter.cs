using System.Runtime.CompilerServices;

namespace MauiGame.Core.Utilities;

/// <summary>
/// Simple FPS counter based on an exponential moving average of frame times.
/// Call <see cref="OnFrame(double)"/> once per rendered frame with frame delta seconds.
/// </summary>
public sealed class FpsCounter
{
    private double emaFrameTime;
    private bool initialized;

    /// <summary>Gets the current frames-per-second estimate.</summary>
    public double Fps
    {
        get
        {
            if (this.emaFrameTime <= 0.0) return 0.0;
            return 1.0 / this.emaFrameTime;
        }
    }

    /// <summary>Smoothing factor in [0,1]. Higher is more responsive, lower is smoother.</summary>
    public double Smoothing { get; set; }

    /// <summary>Create a new FPS counter with a default smoothing of 0.1.</summary>
    public FpsCounter(double smoothing = 0.1)
    {
        if (smoothing <= 0.0 || smoothing > 1.0) throw new ArgumentOutOfRangeException(nameof(smoothing));
        this.Smoothing = smoothing;
        this.emaFrameTime = 0.0;
        this.initialized = false;
    }

    /// <summary>Report a frame with the given delta time in seconds.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnFrame(double deltaSeconds)
    {
        if (deltaSeconds <= 0.0)
        {
            return;
        }

        if (!this.initialized)
        {
            this.emaFrameTime = deltaSeconds;
            this.initialized = true;
            return;
        }

        double alpha = this.Smoothing;
        this.emaFrameTime = alpha * deltaSeconds + (1.0 - alpha) * this.emaFrameTime;
    }
}