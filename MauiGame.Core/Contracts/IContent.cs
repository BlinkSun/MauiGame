namespace MauiGame.Core.Contracts;

/// <summary>
/// Loads and manages asset resources such as textures and fonts.
/// </summary>
public interface IContent : IDisposable
{
    /// <summary>Loads a texture from a given path.</summary>
    Task<ITexture> LoadTextureAsync(string path, CancellationToken cancellationToken);

    /// <summary>Loads a font from a given path.</summary>
    Task<IFont> LoadFontAsync(string path, CancellationToken cancellationToken);
}