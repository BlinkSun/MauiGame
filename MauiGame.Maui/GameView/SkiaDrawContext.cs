using MauiGame.Core.Contracts;
using SkiaSharp;

namespace MauiGame.Maui.GameView;

/// <summary>
/// Draw context backed by SkiaSharp SKCanvas.
/// </summary>
/// <remarks>Create a new Skia draw context.</remarks>
public sealed class SkiaDrawContext(int width, int height, SKCanvas canvas) : IDrawContext
{
    /// <inheritdoc/>
    public int Width { get; } = width;

    /// <inheritdoc/>
    public int Height { get; } = height;

    /// <summary>The underlying Skia canvas for the current frame.</summary>
    public SKCanvas Canvas { get; } = canvas ?? throw new ArgumentNullException(nameof(canvas));
}