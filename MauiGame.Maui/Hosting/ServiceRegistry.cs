namespace MauiGame.Maui.Hosting;

/// <summary>
/// Tiny service registry to share engine services (content, audio, input, etc.).
/// </summary>
public sealed class ServiceRegistry
{
    private readonly Dictionary<Type, object> services;

    /// <summary>Create an empty registry.</summary>
    public ServiceRegistry()
    {
        this.services = [];
    }

    /// <summary>Adds or replaces a service instance for the given interface.</summary>
    public void AddService<TService>(TService instance) where TService : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        this.services[typeof(TService)] = instance;
    }

    /// <summary>Tries to add a service only if not already present.</summary>
    public void TryAddService<TService>(TService instance) where TService : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (!this.services.ContainsKey(typeof(TService)))
        {
            this.services[typeof(TService)] = instance;
        }
    }

    /// <summary>Resolves a service or throws if missing.</summary>
    public TService Get<TService>() where TService : class
    {
        if (this.services.TryGetValue(typeof(TService), out object? value) && value is TService s)
        {
            return s;
        }

        throw new InvalidOperationException($"Service not found: {typeof(TService).FullName}");
    }

    /// <summary>Attempts to resolve a service, returning null if not found.</summary>
    public TService? TryGet<TService>() where TService : class
    {
        if (this.services.TryGetValue(typeof(TService), out object? value) && value is TService s)
        {
            return s;
        }

        return null;
    }
}