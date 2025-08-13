using MauiGame.Core.Contracts;
using MauiGame.Core.Time;

namespace MauiGame.Core.Scenes;

/// <summary>
/// Base scene with safe default implementations for lifecycle methods.
/// Inherit and override as needed.
/// </summary>
/// <remarks>Creates a scene with a given name.</remarks>
public abstract class Scene(string name) : IScene
{
    private bool isLoaded = false;

    /// <summary>Content loading service available to the scene.</summary>
    protected IContent Content { get; private set; } = null!;

    /// <summary>Audio playback service available to the scene.</summary>
    protected IAudio Audio { get; private set; } = null!;

    /// <summary>Input polling service available to the scene.</summary>
    protected IInput Input { get; private set; } = null!;

    /// <summary>Scene manager available to the scene.</summary>
    protected SceneManager SceneManager { get; private set; } = null!;

    /// <inheritdoc/>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <inheritdoc/>
    public bool IsLoaded => this.isLoaded;

    /// <inheritdoc/>
    public virtual void Initialize()
    {
        // Default: nothing to initialize.
    }

    /// <inheritdoc/>
    public virtual async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Default: simulate work so derived scenes can call base then load specifics.
            await Task.CompletedTask.ConfigureAwait(false);
            this.isLoaded = true;
        }
        catch (Exception)
        {
            // Flag remains false if load fails.
            this.isLoaded = false;
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual void Update(GameTime time)
    {
        if (!this.isLoaded)
        {
            return;
        }
        // Default: do nothing.
    }

    /// <inheritdoc/>
    public virtual void Draw(IDrawContext context)
    {
        if (!this.isLoaded)
        {
            return;
        }
        // Default: do nothing.
    }

    /// <inheritdoc/>
    public virtual void Unload()
    {
        this.isLoaded = false;
    }

    /// <summary>Attaches engine services to the scene.</summary>
    /// <param name="sceneManager">Scene manager controlling scene transitions.</param>
    /// <param name="content">Content loading service.</param>
    /// <param name="audio">Audio playback service.</param>
    /// <param name="input">Input polling service.</param>
    internal void AttachServices(SceneManager sceneManager, IContent content, IAudio audio, IInput input)
    {
        SceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Audio = audio ?? throw new ArgumentNullException(nameof(audio));
        Input = input ?? throw new ArgumentNullException(nameof(input));
    }

    /// <summary>Disposes the scene, calling <see cref="Unload"/> by default.</summary>
    public virtual void Dispose()
    {
        try
        {
            Unload();
        }
        catch (Exception)
        {
            // Intentionally swallow exceptions during Dispose.
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}