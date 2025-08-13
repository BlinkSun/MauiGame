namespace MauiGame.Core;

using MauiGame.Core.Contracts;
using MauiGame.Core.Scenes;
using MauiGame.Core.Time;

/// <summary>
/// Base game class providing access to core engine services and scene management.
/// </summary>
public abstract class Game : IGame
{
    /// <summary>Initializes a new instance of the <see cref="Game"/> class.</summary>
    protected Game()
    {
        Scenes = new SceneManager(logger: Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
        Time = new GameTime();
    }

    /// <summary>Content loading service.</summary>
    protected IContent Content { get; private set; } = null!;

    /// <summary>Audio playback service.</summary>
    protected IAudio Audio { get; private set; } = null!;

    /// <summary>Input polling service.</summary>
    protected IInput Input { get; private set; } = null!;

    /// <summary>Scene manager for organizing gameplay.</summary>
    protected SceneManager Scenes { get; }

    /// <summary>Time tracking for the game.</summary>
    protected GameTime Time { get; }

    /// <summary>Wires engine services; called by the host.</summary>
    /// <param name="content">Content loading service.</param>
    /// <param name="audio">Audio playback service.</param>
    /// <param name="input">Input polling service.</param>
    public void AttachServices(IContent content, IAudio audio, IInput input)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Audio = audio ?? throw new ArgumentNullException(nameof(audio));
        Input = input ?? throw new ArgumentNullException(nameof(input));
    }

    /// <inheritdoc />
    public virtual void Initialize()
    {
    }

    /// <inheritdoc />
    public virtual async Task LoadAsync(CancellationToken cancellationToken)
    {
        await Scenes.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual void Update(double deltaSeconds)
    {
        Time.Advance(deltaSeconds, 0.0);
        Scenes.Update(Time);
    }

    /// <inheritdoc />
    public virtual void Draw(IDrawContext context)
    {
        Scenes.Draw(context);
    }
}

