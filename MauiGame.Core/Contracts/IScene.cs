using MauiGame.Core.Time;

namespace MauiGame.Core.Contracts;

/// <summary>
/// Scene interface that encapsulates a part of the game (menu, level, etc.).
/// </summary>
public interface IScene : IDisposable
{
    /// <summary>Scene unique name (for debugging and lookup).</summary>
    string Name { get; }

    /// <summary>True after all assets have been loaded.</summary>
    bool IsLoaded { get; }

    /// <summary>Initialize lightweight state.</summary>
    void Initialize();

    /// <summary>Asynchronously load content and heavy resources.</summary>
    Task LoadAsync(CancellationToken cancellationToken);

    /// <summary>Update logic using a fixed time step.</summary>
    /// <param name="time">GameTime information (delta, total).</param>
    void Update(GameTime time);

    /// <summary>Render the scene.</summary>
    void Draw(IDrawContext context);

    /// <summary>Free resources not managed by the content pipeline.</summary>
    void Unload();
}