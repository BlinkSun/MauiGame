using MauiGame.Core.Contracts;
using MauiGame.Core.Time;
using Microsoft.Extensions.Logging;

namespace MauiGame.Core.Scenes;

/// <summary>
/// Manages a stack of scenes (push/pop/replace). The top-most scene receives Update/Draw.
/// </summary>
/// <remarks>Create a new SceneManager.</remarks>
public sealed class SceneManager(ILogger? logger = null) : IDisposable
{
    private readonly Stack<IScene> stack = new();
    private readonly ILogger logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    private IContent? content;
    private IAudio? audio;
    private IInput? input;

    /// <summary>Returns the scene on top of the stack, or null if none.</summary>
    public IScene? Current => this.stack.Count > 0 ? this.stack.Peek() : null;

    /// <summary>Attaches engine services used to inject into scenes.</summary>
    /// <param name="content">Content loading service.</param>
    /// <param name="audio">Audio playback service.</param>
    /// <param name="input">Input polling service.</param>
    public void AttachServices(IContent content, IAudio audio, IInput input)
    {
        this.content = content ?? throw new ArgumentNullException(nameof(content));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
        this.input = input ?? throw new ArgumentNullException(nameof(input));
    }

    /// <summary>Pushes a new scene on the stack (it becomes current).</summary>
    public void Push(IScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (scene is Scene typedScene)
        {
            if (this.content == null || this.audio == null || this.input == null)
            {
                throw new InvalidOperationException("Services have not been attached to the SceneManager.");
            }
            typedScene.AttachServices(this, this.content, this.audio, this.input);
        }
        this.stack.Push(scene);
        this.logger.LogInformation("Scene pushed: {Name}", scene.Name);
    }

    /// <summary>Pops the current scene, disposing it.</summary>
    public void Pop()
    {
        if (this.stack.Count == 0)
        {
            return;
        }

        IScene scene = this.stack.Pop();
        try
        {
            scene.Dispose();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error while disposing scene {Name}", scene.Name);
        }
        this.logger.LogInformation("Scene popped: {Name}", scene.Name);
    }

    /// <summary>Replaces the current scene with another scene.</summary>
    public void Replace(IScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        Pop();
        Push(scene);
    }

    /// <summary>Initializes and loads the current scene if not yet loaded.</summary>
    public async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        IScene? current = this.Current;
        if (current == null)
        {
            return;
        }

        if (!current.IsLoaded)
        {
            try
            {
                current.Initialize();
                await current.LoadAsync(cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation("Scene loaded: {Name}", current.Name);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to load scene {Name}", current.Name);
                throw;
            }
        }
    }

    /// <summary>Updates the current scene.</summary>
    public void Update(GameTime time)
    {
        IScene? current = this.Current;
        if (current == null)
        {
            return;
        }

        try
        {
            current.Update(time);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Update failed in scene {Name}", current.Name);
            throw;
        }
    }

    /// <summary>Draws the current scene.</summary>
    public void Draw(IDrawContext context)
    {
        IScene? current = this.Current;
        if (current == null)
        {
            return;
        }

        try
        {
            current.Draw(context);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Draw failed in scene {Name}", current.Name);
            throw;
        }
    }

    /// <summary>Disposes all scenes on the stack.</summary>
    public void Dispose()
    {
        while (this.stack.Count > 0)
        {
            IScene scene = this.stack.Pop();
            try
            {
                scene.Dispose();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while disposing scene {Name}", scene.Name);
            }
        }
    }
}