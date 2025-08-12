namespace MauiGame.Core.Contracts;

/// <summary>
/// Abstraction for a platform-specific drawing surface.
/// </summary>
public interface IDrawContext
{
    /// <summary>Width in pixels.</summary>
    int Width { get; }

    /// <summary>Height in pixels.</summary>
    int Height { get; }
}