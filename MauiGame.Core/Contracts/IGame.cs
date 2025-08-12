namespace MauiGame.Core.Contracts;

/// <summary>
/// Represents a game application lifecycle. The host drives these methods.
/// </summary>
public interface IGame
{
    /// <summary>Initialize non-asset, non-graphics state.</summary>
    void Initialize();

    /// <summary>Asynchronously load assets and heavyweight resources.</summary>
    Task LoadAsync(CancellationToken cancellationToken);

    /// <summary>Advance simulation with a fixed delta time (seconds).</summary>
    /// <param name="deltaSeconds">Fixed delta time in seconds.</param>
    void Update(double deltaSeconds);

    /// <summary>Render the current frame.</summary>
    /// <param name="context">Platform drawing context.</param>
    void Draw(IDrawContext context);
}
