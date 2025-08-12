using System.Numerics;

namespace MauiGame.Core.Contracts;

/// <summary>
/// Pollable input snapshot abstractions.
/// Platform layer updates the states, game reads snapshots in Update.
/// </summary>
public interface IInput
{
    /// <summary>Returns the current immutable keyboard state.</summary>
    KeyboardState GetKeyboardState();

    /// <summary>Returns the current immutable mouse/pointer state.</summary>
    MouseState GetMouseState();

    /// <summary>Returns the current immutable touch state.</summary>
    TouchState GetTouchState();
}

/// <summary>Subset of keys sufficient for many 2D games. Extend as needed.</summary>
public enum Key
{
    Unknown = 0,
    A, B, C, D, E, F, G, H, I, J, K, L, M,
    N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
    Left, Right, Up, Down,
    Space, Enter, Escape, Shift, Control, Alt, Tab
}

/// <summary>Mouse buttons supported by the engine.</summary>
[Flags]
public enum MouseButtons
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 4,
    X1 = 8,
    X2 = 16
}

/// <summary>Represents a snapshot of the keyboard at a point in time.</summary>
public readonly struct KeyboardState
{
    private readonly HashSet<Key> pressed;

    /// <summary>Create a snapshot with a pressed set.</summary>
    public KeyboardState(IEnumerable<Key> pressedKeys)
    {
        ArgumentNullException.ThrowIfNull(pressedKeys);

        this.pressed = [.. pressedKeys];
    }

    /// <summary>Returns whether the key is currently pressed.</summary>
    public bool IsDown(Key key) => this.pressed != null && this.pressed.Contains(key);
}

/// <summary>Represents a snapshot of mouse/pointer state.</summary>
/// <remarks>Creates a new mouse state snapshot.</remarks>
public readonly struct MouseState(in Vector2 position, MouseButtons buttons, float scrollDelta)
{
    /// <summary>Current mouse position in pixels.</summary>
    public Vector2 Position { get; } = position;

    /// <summary>Buttons currently pressed.</summary>
    public MouseButtons Buttons { get; } = buttons;

    /// <summary>Scroll delta since last snapshot (units are platform-defined).</summary>
    public float ScrollDelta { get; } = scrollDelta;

    /// <summary>Returns true if the given button bit is set.</summary>
    public bool IsDown(MouseButtons button) => (this.Buttons & button) == button;
}

/// <summary>Represents a single touch contact.</summary>
public readonly struct TouchPoint(int id, in Vector2 position, TouchPhase phase)
{
    public int Id { get; } = id;
    public Vector2 Position { get; } = position;
    public TouchPhase Phase { get; } = phase;
}

/// <summary>Touch lifecycle phase.</summary>
public enum TouchPhase
{
    Began,
    Moved,
    Stationary,
    Ended,
    Canceled
}

/// <summary>Represents the current set of touches.</summary>
public readonly struct TouchState(IReadOnlyList<TouchPoint> touches)
{
    private readonly IReadOnlyList<TouchPoint> touches = touches ?? [];

    /// <summary>Enumerates active touches.</summary>
    public IReadOnlyList<TouchPoint> Touches => this.touches ?? [];
}
