using System.Diagnostics.CodeAnalysis;

namespace MauiGame.Core.Math;

/// <summary>
/// Lightweight float-based rectangle structure similar to XNA/MonoGame Rectangle.
/// </summary>
public readonly struct RectangleF(float x, float y, float width, float height) : IEquatable<RectangleF>
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Width { get; } = width;
    public float Height { get; } = height;

    public float Left => this.X;
    public float Top => this.Y;
    public float Right => this.X + this.Width;
    public float Bottom => this.Y + this.Height;

    public bool Contains(float x, float y) => x >= this.Left && x <= this.Right && y >= this.Top && y <= this.Bottom;

    public bool Intersects(in RectangleF other) => !(other.Left > this.Right || other.Right < this.Left || other.Top > this.Bottom || other.Bottom < this.Top);

    public bool Equals(RectangleF other) => this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Width.Equals(other.Width) && this.Height.Equals(other.Height);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is RectangleF r)
        {
            return Equals(r);
        }

        return false;
    }

    public override int GetHashCode() => HashCode.Combine(this.X, this.Y, this.Width, this.Height);

    public static bool operator ==(RectangleF a, RectangleF b) => a.Equals(b);
    public static bool operator !=(RectangleF a, RectangleF b) => !a.Equals(b);

    public override string ToString() => $"RectangleF(X={this.X}, Y={this.Y}, W={this.Width}, H={this.Height})";
}