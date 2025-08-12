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