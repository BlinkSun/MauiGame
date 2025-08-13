namespace MauiGame.Core;

using MauiGame.Core.Contracts;
using MauiGame.Core.Scenes;
using MauiGame.Core.Time;
using Microsoft.Extensions.Logging;

/// <summary>
/// Base game class providing access to core engine services and scene management.
/// </summary>
public abstract class Game : IGame
{
    /// <summary>Initializes a new instance of the <see cref="Game"/> class.</summary>
    protected Game()
    {
        this.Scenes = new SceneManager(logger: Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
        this.Time = new GameTime();
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
        this.Content = content ?? throw new ArgumentNullException(nameof(content));
        this.Audio = audio ?? throw new ArgumentNullException(nameof(audio));
        this.Input = input ?? throw new ArgumentNullException(nameof(input));
    }

    /// <inheritdoc />
    public virtual void Initialize()
    {
    }

    /// <inheritdoc />
    public virtual async Task LoadAsync(CancellationToken cancellationToken)
    {
        await this.Scenes.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual void Update(double deltaSeconds)
    {
        this.Time.Advance(deltaSeconds, 0.0);
        this.Scenes.Update(this.Time);
    }

    /// <inheritdoc />
    public virtual void Draw(IDrawContext context)
    {
        this.Scenes.Draw(context);
    }
}

