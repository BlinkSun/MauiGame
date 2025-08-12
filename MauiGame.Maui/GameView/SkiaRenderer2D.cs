using MauiGame.Core.Contracts;
using MauiGame.Core.Math;
using SkiaSharp;
using System.Numerics;

namespace MauiGame.Maui.GameView;

/// <summary>
/// Simple 2D renderer using SkiaSharp. Create per-frame with the current canvas.
/// </summary>
/// <remarks>Create a renderer for the given canvas.</remarks>
public sealed partial class SkiaRenderer2D(SKCanvas canvas) : IRenderer2D, IDisposable
{
    private readonly SKCanvas canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
    private bool began = false;

    /// <inheritdoc/>
    public void Begin(in Matrix3x2 transform)
    {
        if (this.began) throw new InvalidOperationException("Begin called twice.");
        this.began = true;

        this.canvas.Save();
        this.canvas.SetMatrix(ToSkMatrix(transform));
        this.canvas.Clear(SKColors.Black);
    }

    /// <inheritdoc/>
    public void DrawSprite(ITexture texture, in Vector2 position, in Vector2 origin, in Vector2 scale, float rotationRadians, in RectangleF? sourceRect)
    {
        if (!this.began) throw new InvalidOperationException("Begin must be called before DrawSprite.");

        Texture2D skTex = (Texture2D)texture;
        SKRect src = sourceRect.HasValue
            ? new SKRect(sourceRect.Value.Left, sourceRect.Value.Top, sourceRect.Value.Right, sourceRect.Value.Bottom)
            : new SKRect(0, 0, skTex.Bitmap.Width, skTex.Bitmap.Height);

        SKPoint center = new(origin.X, origin.Y);

        this.canvas.Save();
        this.canvas.Translate(position.X, position.Y);
        this.canvas.RotateRadians(rotationRadians, center.X, center.Y);
        this.canvas.Scale(scale.X, scale.Y, center.X, center.Y);
        this.canvas.DrawBitmap(skTex.Bitmap, src, new SKRect(-center.X, -center.Y, -center.X + src.Width, -center.Y + src.Height));
        this.canvas.Restore();
    }

    /// <inheritdoc/>
    public void DrawText(Core.Contracts.IFont font, string text, in Vector2 position, float size, float rotationRadians)
    {
        if (!this.began) throw new InvalidOperationException("Begin must be called before DrawText.");

        SKFont skFont = new(((Font)font).Typeface, size) { Edging = SKFontEdging.Alias };
        using SKPaint paint = new() { IsAntialias = true, Color = SKColors.White };
        this.canvas.Save();
        this.canvas.Translate(position.X, position.Y);
        this.canvas.RotateRadians(rotationRadians);
        this.canvas.DrawText(text ?? string.Empty, 0, 0, SKTextAlign.Left, skFont, paint);
        this.canvas.Restore();
    }

    /// <inheritdoc/>
    public void End()
    {
        if (!this.began) return;
        this.began = false;
        this.canvas.Restore();
    }

    /// <summary>Converts a System.Numerics.Matrix3x2 to an SKMatrix.</summary>
    private static SKMatrix ToSkMatrix(in Matrix3x2 m)
    {
        return new SKMatrix
        {
            ScaleX = m.M11,
            SkewX = m.M12,
            TransX = m.M31,
            SkewY = m.M21,
            ScaleY = m.M22,
            TransY = m.M32,
            Persp0 = 0,
            Persp1 = 0,
            Persp2 = 1
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Nothing to dispose here; textures/fonts are separate resources.
    }

    // ===== Concrete resource types for Skia backend =====

    /// <summary>Concrete texture implementation for Skia.</summary>
    public sealed partial class Texture2D(SKBitmap bitmap) : ITexture
    {
        public SKBitmap Bitmap { get; } = bitmap ?? throw new ArgumentNullException(nameof(bitmap));

        public int Width => this.Bitmap.Width;
        public int Height => this.Bitmap.Height;

        public void Dispose()
        {
            try { this.Bitmap.Dispose(); } catch (Exception) { }
        }
    }

    /// <summary>Concrete font implementation for Skia.</summary>
    public sealed partial class Font(SKTypeface typeface) : Core.Contracts.IFont
    {
        public SKTypeface Typeface { get; } = typeface ?? throw new ArgumentNullException(nameof(typeface));

        public void Dispose()
        {
            try { this.Typeface.Dispose(); } catch (Exception) { }
        }
    }
}