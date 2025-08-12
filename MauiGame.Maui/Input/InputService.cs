using MauiGame.Core.Contracts;
using SkiaSharp.Views.Maui;
using System.Numerics;

namespace MauiGame.Maui.Input;

/// <summary>
/// Collects and snapshots input state from MAUI/Skia events.
/// Game reads immutable snapshots via IInput.
/// </summary>
public sealed class InputService : IInput
{
    private readonly HashSet<Core.Contracts.Key> pressedKeys;
    private readonly Lock sync;
    private Vector2 mousePosition;
    private Core.Contracts.MouseButtons mouseButtons;
    private float scrollDelta;
    private List<Core.Contracts.TouchPoint> touches;

    /// <summary>Create a new input service.</summary>
    public InputService()
    {
        this.pressedKeys = [];
        this.sync = new Lock();
        this.mousePosition = Vector2.Zero;
        this.mouseButtons = Core.Contracts.MouseButtons.None;
        this.scrollDelta = 0.0f;
        this.touches = new List<Core.Contracts.TouchPoint>(4);
    }

    /// <inheritdoc/>
    public Core.Contracts.KeyboardState GetKeyboardState()
    {
        lock (this.sync)
        {
            return new Core.Contracts.KeyboardState([.. this.pressedKeys]);
        }
    }

    /// <inheritdoc/>
    public Core.Contracts.MouseState GetMouseState()
    {
        lock (this.sync)
        {
            Core.Contracts.MouseState state = new(this.mousePosition, this.mouseButtons, this.scrollDelta);
            this.scrollDelta = 0.0f; // reset deltas per read
            return state;
        }
    }

    /// <inheritdoc/>
    public Core.Contracts.TouchState GetTouchState()
    {
        lock (this.sync)
        {
            Core.Contracts.TouchState state = new([.. this.touches]);
            return state;
        }
    }

    // ===== Forwarders to be called from GameView or platform layer =====

    /// <summary>Map a KeyCodes to engine Key and set as down.</summary>
    public void HandleKeyDown(KeyCodes key)
    {
        Core.Contracts.Key mapped = MapKey(key);
        if (mapped == Core.Contracts.Key.Unknown) return;

        lock (this.sync)
        {
            this.pressedKeys.Add(mapped);
        }
    }

    /// <summary>Map a KeyCodes to engine Key and set as up.</summary>
    public void HandleKeyUp(KeyCodes key)
    {
        Core.Contracts.Key mapped = MapKey(key);
        if (mapped == Core.Contracts.Key.Unknown) return;

        lock (this.sync)
        {
            this.pressedKeys.Remove(mapped);
        }
    }

    /// <summary>Handle mouse move (desktop).</summary>
    public void HandleMouseMove(float x, float y)
    {
        lock (this.sync)
        {
            this.mousePosition = new Vector2(x, y);
        }
    }

    /// <summary>Handle mouse down (desktop).</summary>
    public void HandleMouseDown(SKMouseButton buttons)
    {
        lock (this.sync)
        {
            if (buttons.HasFlag(SKMouseButton.Left)) this.mouseButtons |= Core.Contracts.MouseButtons.Left;
            if (buttons.HasFlag(SKMouseButton.Right)) this.mouseButtons |= Core.Contracts.MouseButtons.Right;
            if (buttons.HasFlag(SKMouseButton.Middle)) this.mouseButtons |= Core.Contracts.MouseButtons.Middle;
        }
    }

    /// <summary>Handle mouse up (desktop).</summary>
    public void HandleMouseUp(SKMouseButton buttons)
    {
        lock (this.sync)
        {
            if (buttons.HasFlag(SKMouseButton.Left)) this.mouseButtons &= ~Core.Contracts.MouseButtons.Left;
            if (buttons.HasFlag(SKMouseButton.Right)) this.mouseButtons &= ~Core.Contracts.MouseButtons.Right;
            if (buttons.HasFlag(SKMouseButton.Middle)) this.mouseButtons &= ~Core.Contracts.MouseButtons.Middle;
        }
    }

    /// <summary>Handle scroll (optional – call if you wire to wheel events).</summary>
    public void HandleScroll(float delta)
    {
        lock (this.sync)
        {
            this.scrollDelta += delta;
        }
    }

    /// <summary>Handle touch events from SKGLView.</summary>
    public void HandleTouch(SKTouchEventArgs e)
    {
        lock (this.sync)
        {
            // Update mouse position mirror for convenience
            this.mousePosition = new Vector2((float)e.Location.X, (float)e.Location.Y);

            Core.Contracts.TouchPhase phase = e.ActionType switch
            {
                SKTouchAction.Pressed => Core.Contracts.TouchPhase.Began,
                SKTouchAction.Moved => Core.Contracts.TouchPhase.Moved,
                SKTouchAction.Released => Core.Contracts.TouchPhase.Ended,
                SKTouchAction.Cancelled => Core.Contracts.TouchPhase.Canceled,
                _ => Core.Contracts.TouchPhase.Stationary
            };

            // Replace or update touch with same id
            int idx = this.touches.FindIndex(t => t.Id == e.Id);
            Core.Contracts.TouchPoint tp = new((int)e.Id, this.mousePosition, phase);

            if (phase == Core.Contracts.TouchPhase.Ended || phase == Core.Contracts.TouchPhase.Canceled)
            {
                if (idx >= 0) this.touches.RemoveAt(idx);
            }
            else
            {
                if (idx >= 0) this.touches[idx] = tp;
                else this.touches.Add(tp);
            }

            // For single-touch, emulate left mouse button for convenience
            if (this.touches.Count > 0) this.mouseButtons |= Core.Contracts.MouseButtons.Left;
            else this.mouseButtons &= ~Core.Contracts.MouseButtons.Left;
        }
    }

    private static Core.Contracts.Key MapKey(KeyCodes key)
    {
        // Simple 1:1 mapping where names match
        switch (key)
        {
            case KeyCodes.A: return Core.Contracts.Key.A;
            case KeyCodes.B: return Core.Contracts.Key.B;
            case KeyCodes.C: return Core.Contracts.Key.C;
            case KeyCodes.D: return Core.Contracts.Key.D;
            case KeyCodes.E: return Core.Contracts.Key.E;
            case KeyCodes.F: return Core.Contracts.Key.F;
            case KeyCodes.G: return Core.Contracts.Key.G;
            case KeyCodes.H: return Core.Contracts.Key.H;
            case KeyCodes.I: return Core.Contracts.Key.I;
            case KeyCodes.J: return Core.Contracts.Key.J;
            case KeyCodes.K: return Core.Contracts.Key.K;
            case KeyCodes.L: return Core.Contracts.Key.L;
            case KeyCodes.M: return Core.Contracts.Key.M;
            case KeyCodes.N: return Core.Contracts.Key.N;
            case KeyCodes.O: return Core.Contracts.Key.O;
            case KeyCodes.P: return Core.Contracts.Key.P;
            case KeyCodes.Q: return Core.Contracts.Key.Q;
            case KeyCodes.R: return Core.Contracts.Key.R;
            case KeyCodes.S: return Core.Contracts.Key.S;
            case KeyCodes.T: return Core.Contracts.Key.T;
            case KeyCodes.U: return Core.Contracts.Key.U;
            case KeyCodes.V: return Core.Contracts.Key.V;
            case KeyCodes.W: return Core.Contracts.Key.W;
            case KeyCodes.X: return Core.Contracts.Key.X;
            case KeyCodes.Y: return Core.Contracts.Key.Y;
            case KeyCodes.Z: return Core.Contracts.Key.Z;

            case KeyCodes.D0: return Core.Contracts.Key.D0;
            case KeyCodes.D1: return Core.Contracts.Key.D1;
            case KeyCodes.D2: return Core.Contracts.Key.D2;
            case KeyCodes.D3: return Core.Contracts.Key.D3;
            case KeyCodes.D4: return Core.Contracts.Key.D4;
            case KeyCodes.D5: return Core.Contracts.Key.D5;
            case KeyCodes.D6: return Core.Contracts.Key.D6;
            case KeyCodes.D7: return Core.Contracts.Key.D7;
            case KeyCodes.D8: return Core.Contracts.Key.D8;
            case KeyCodes.D9: return Core.Contracts.Key.D9;

            case KeyCodes.Left: return Core.Contracts.Key.Left;
            case KeyCodes.Right: return Core.Contracts.Key.Right;
            case KeyCodes.Up: return Core.Contracts.Key.Up;
            case KeyCodes.Down: return Core.Contracts.Key.Down;

            case KeyCodes.Space: return Core.Contracts.Key.Space;
            case KeyCodes.Enter: return Core.Contracts.Key.Enter;
            case KeyCodes.Escape: return Core.Contracts.Key.Escape;
            case KeyCodes.Shift: return Core.Contracts.Key.Shift;
            case KeyCodes.Control: return Core.Contracts.Key.Control;
            case KeyCodes.Alt: return Core.Contracts.Key.Alt;
            case KeyCodes.Tab: return Core.Contracts.Key.Tab;

            default: return Core.Contracts.Key.Unknown;
        }
    }
}
