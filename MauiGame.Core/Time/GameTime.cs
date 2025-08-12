namespace MauiGame.Core.Time;

/// <summary>
/// Carries timing information for fixed-step updates and optional interpolation.
/// </summary>
/// <remarks>Creates a new <see cref="GameTime"/>.</remarks>
public sealed class GameTime(double totalSeconds = 0.0, double deltaSeconds = 0.0, double alpha = 0.0)
{
    /// <summary>Total elapsed time in seconds since the start.</summary>
    public double TotalSeconds { get; private set; } = totalSeconds;

    /// <summary>Fixed delta time in seconds for this step.</summary>
    public double DeltaSeconds { get; private set; } = deltaSeconds;

    /// <summary>Optional interpolation alpha [0..1] for rendering between steps.</summary>
    public double Alpha { get; private set; } = alpha;

    /// <summary>Advances time by a fixed delta and sets interpolation alpha.</summary>
    public void Advance(double fixedDeltaSeconds, double alpha)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fixedDeltaSeconds, 0.0);
        alpha = System.Math.Clamp(alpha, 0.0, 1.0);

        this.DeltaSeconds = fixedDeltaSeconds;
        this.TotalSeconds += fixedDeltaSeconds;
        this.Alpha = alpha;
    }
}