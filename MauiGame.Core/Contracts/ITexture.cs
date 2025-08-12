namespace MauiGame.Core.Contracts;

/// <summary>
/// Represents a 2D texture resource.
/// </summary>
public interface ITexture : IDisposable
{
    /// <summary>Texture width in pixels.</summary>
    int Width { get; }

    /// <summary>Texture height in pixels.</summary>
    int Height { get; }
}