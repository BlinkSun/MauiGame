using MauiGame.Core.Contracts;
using System.Numerics;

namespace SampleGame.Game;

/// <summary>
/// Simple player controller supporting keyboard (WASD/Arrows) and touch drag.
/// </summary>
public sealed class PlayerController(IInput input, float speed = 180.0f)
{
    private readonly IInput input = input ?? throw new ArgumentNullException(nameof(input));
    private Vector2 velocity = Vector2.Zero;

    /// <summary>Computes a movement delta for the frame.</summary>
    public Vector2 Update(double deltaSeconds, in Vector2 currentPosition)
    {
        MauiGame.Core.Contracts.KeyboardState keys = this.input.GetKeyboardState();
        MauiGame.Core.Contracts.TouchState touches = this.input.GetTouchState();
        MauiGame.Core.Contracts.MouseState mouse = this.input.GetMouseState();

        this.velocity = Vector2.Zero;

        // Keyboard
        if (keys.IsDown(Key.W) || keys.IsDown(Key.Up)) this.velocity.Y -= 1.0f;
        if (keys.IsDown(Key.S) || keys.IsDown(Key.Down)) this.velocity.Y += 1.0f;
        if (keys.IsDown(Key.A) || keys.IsDown(Key.Left)) this.velocity.X -= 1.0f;
        if (keys.IsDown(Key.D) || keys.IsDown(Key.Right)) this.velocity.X += 1.0f;

        // Touch: if a touch exists, move towards it
        if (touches.Touches.Count > 0)
        {
            // take first touch
            System.Numerics.Vector2 target = touches.Touches[0].Position;
            System.Numerics.Vector2 dir = target - currentPosition;
            float len = dir.Length();
            if (len > 1.0f)
            {
                dir /= len;
                this.velocity = dir;
            }
        }

        if (this.velocity.LengthSquared() > 1e-5f)
        {
            this.velocity = Vector2.Normalize(this.velocity);
        }

        float dt = (float)deltaSeconds;
        System.Numerics.Vector2 delta = this.velocity * speed * dt;
        return delta;
    }
}