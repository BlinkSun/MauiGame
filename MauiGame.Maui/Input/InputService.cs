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

    private static readonly Dictionary<KeyCodes, Core.Contracts.Key> KeyMap = new()
    {
        { KeyCodes.A, Core.Contracts.Key.A },
        { KeyCodes.B, Core.Contracts.Key.B },
        { KeyCodes.C, Core.Contracts.Key.C },
        { KeyCodes.D, Core.Contracts.Key.D },
        { KeyCodes.E, Core.Contracts.Key.E },
        { KeyCodes.F, Core.Contracts.Key.F },
        { KeyCodes.G, Core.Contracts.Key.G },
        { KeyCodes.H, Core.Contracts.Key.H },
        { KeyCodes.I, Core.Contracts.Key.I },
        { KeyCodes.J, Core.Contracts.Key.J },
        { KeyCodes.K, Core.Contracts.Key.K },
        { KeyCodes.L, Core.Contracts.Key.L },
        { KeyCodes.M, Core.Contracts.Key.M },
        { KeyCodes.N, Core.Contracts.Key.N },
        { KeyCodes.O, Core.Contracts.Key.O },
        { KeyCodes.P, Core.Contracts.Key.P },
        { KeyCodes.Q, Core.Contracts.Key.Q },
        { KeyCodes.R, Core.Contracts.Key.R },
        { KeyCodes.S, Core.Contracts.Key.S },
        { KeyCodes.T, Core.Contracts.Key.T },
        { KeyCodes.U, Core.Contracts.Key.U },
        { KeyCodes.V, Core.Contracts.Key.V },
        { KeyCodes.W, Core.Contracts.Key.W },
        { KeyCodes.X, Core.Contracts.Key.X },
        { KeyCodes.Y, Core.Contracts.Key.Y },
        { KeyCodes.Z, Core.Contracts.Key.Z },
        { KeyCodes.D0, Core.Contracts.Key.D0 },
        { KeyCodes.D1, Core.Contracts.Key.D1 },
        { KeyCodes.D2, Core.Contracts.Key.D2 },
        { KeyCodes.D3, Core.Contracts.Key.D3 },
        { KeyCodes.D4, Core.Contracts.Key.D4 },
        { KeyCodes.D5, Core.Contracts.Key.D5 },
        { KeyCodes.D6, Core.Contracts.Key.D6 },
        { KeyCodes.D7, Core.Contracts.Key.D7 },
        { KeyCodes.D8, Core.Contracts.Key.D8 },
        { KeyCodes.D9, Core.Contracts.Key.D9 },
        { KeyCodes.Left, Core.Contracts.Key.Left },
        { KeyCodes.Right, Core.Contracts.Key.Right },
        { KeyCodes.Up, Core.Contracts.Key.Up },
        { KeyCodes.Down, Core.Contracts.Key.Down },
        { KeyCodes.Space, Core.Contracts.Key.Space },
        { KeyCodes.Enter, Core.Contracts.Key.Enter },
        { KeyCodes.Escape, Core.Contracts.Key.Escape },
        { KeyCodes.Shift, Core.Contracts.Key.Shift },
        { KeyCodes.Control, Core.Contracts.Key.Control },
        { KeyCodes.Alt, Core.Contracts.Key.Alt },
        { KeyCodes.Tab, Core.Contracts.Key.Tab }
    };

    private static Core.Contracts.Key MapKey(KeyCodes key)
    {
        return KeyMap.TryGetValue(key, out Core.Contracts.Key mapped)
            ? mapped
            : Core.Contracts.Key.Unknown;
    }
}
