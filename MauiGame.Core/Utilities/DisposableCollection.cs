namespace MauiGame.Core.Utilities;

/// <summary>
/// Helper that aggregates disposables and disposes them together.
/// </summary>
public sealed class DisposableCollection : IDisposable
{
    private readonly List<IDisposable> items;
    private bool disposed;

    /// <summary>Creates an empty disposable collection.</summary>
    public DisposableCollection()
    {
        this.items = [];
        this.disposed = false;
    }

    /// <summary>Adds a disposable item to the collection.</summary>
    public void Add(IDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);
        ObjectDisposedException.ThrowIf(this.disposed, nameof(DisposableCollection));

        this.items.Add(disposable);
    }

    /// <summary>Disposes all items and clears the list.</summary>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        for (int i = this.items.Count - 1; i >= 0; i--)
        {
            try
            {
                this.items[i].Dispose();
            }
            catch
            {
                // Intentionally swallow exceptions from Dispose.
            }
        }

        this.items.Clear();
    }
}