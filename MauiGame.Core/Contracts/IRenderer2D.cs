using MauiGame.Core.Math;
using System.Numerics;

namespace MauiGame.Core.Contracts;

/// <summary>
/// Minimal 2D renderer abstraction used by the engine and games.
/// </summary>
public interface IRenderer2D
{
    /// <summary>Begins a 2D batch with an optional camera transform.</summary>
    /// <param name="transform">Transform matrix applied to subsequent draw calls.</param>
    void Begin(in Matrix3x2 transform);

    /// <summary>Draws a textured sprite with optional source rectangle.</summary>
    void DrawSprite(ITexture texture,
                    in Vector2 position,
                    in Vector2 origin,
                    in Vector2 scale,
                    float rotationRadians,
                    in RectangleF? sourceRect);

    /// <summary>Draws text using a previously loaded font.</summary>
    void DrawText(IFont font,
                  string text,
                  in Vector2 position,
                  float size,
                  float rotationRadians);

    /// <summary>Ends the batch and flushes draw calls.</summary>
    void End();
}