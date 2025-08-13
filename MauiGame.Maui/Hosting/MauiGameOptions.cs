namespace MauiGame.Maui.Hosting;

/// <summary>
/// Configuration options for the MAUI game engine.
/// </summary>
public sealed class MauiGameOptions
{
    /// <summary>Desired frames per second for the game loop.</summary>
    public double TargetFps { get; set; } = 60.0;
}

