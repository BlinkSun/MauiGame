using MauiGame.Core.Contracts;
using MauiGame.Maui.GameView;
using SkiaSharp;

namespace MauiGame.Maui.Content;

/// <summary>
/// Loads textures and fonts from the MAUI packaged application assets.
/// </summary>
public sealed partial class ContentManager : IContent
{
    private readonly Dictionary<string, object> cache;

    /// <summary>Create a new content manager.</summary>
    public ContentManager()
    {
        this.cache = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<ITexture> LoadTextureAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path must be provided.", nameof(path));

            if (this.cache.TryGetValue(path, out object? cached) && cached is ITexture t)
            {
                return t;
            }

            using Stream stream = await FileSystem.OpenAppPackageFileAsync(path).ConfigureAwait(false);
            using SKManagedStream skStream = new(stream);
            SKBitmap bitmap = SKBitmap.Decode(skStream) ?? throw new InvalidOperationException($"Failed to decode image: {path}");

            SkiaRenderer2D.Texture2D texture = new(bitmap);
            this.cache[path] = texture;
            return texture;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Core.Contracts.IFont> LoadFontAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path must be provided.", nameof(path));

            if (this.cache.TryGetValue(path, out object? cached) && cached is Core.Contracts.IFont f)
            {
                return f;
            }

            using Stream stream = await FileSystem.OpenAppPackageFileAsync(path).ConfigureAwait(false);
            using MemoryStream ms = new();
            await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            ms.Position = 0;

            SKTypeface typeface = SKTypeface.FromStream(ms) ?? throw new InvalidOperationException($"Failed to load font: {path}");
            SkiaRenderer2D.Font font = new(typeface);
            this.cache[path] = font;
            return font;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (KeyValuePair<string, object> kv in this.cache)
        {
            try
            {
                if (kv.Value is IDisposable d)
                {
                    d.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }
        this.cache.Clear();
    }
}