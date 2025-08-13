using MauiGame.Core;
using MauiGame.Core.Contracts;
using MauiGame.Core.Time;
using MauiGame.Maui.Audio;
using MauiGame.Maui.Content;
using MauiGame.Maui.Input;
using Microsoft.Extensions.Logging;

namespace MauiGame.Maui.Hosting;

/// <summary>
/// Orchestrates the game, services and the update/draw cycle.
/// </summary>
public sealed partial class GameHost : IDisposable
{
    private readonly IGame game;
    private readonly ServiceRegistry services;
    private readonly ILogger logger;
    private readonly GameTime time;

    /// <summary>Interpolation alpha in [0..1] for rendering between fixed updates.</summary>
    public double InterpolationAlpha { get; set; }

    /// <summary>Create a new game host with default services (Content, Audio, Input if not provided).</summary>
    public GameHost(IGame game, ServiceRegistry services, ILogger? logger = null)
    {
        this.game = game ?? throw new ArgumentNullException(nameof(game));
        this.services = services ?? throw new ArgumentNullException(nameof(services));
        this.logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        this.time = new GameTime();
        this.InterpolationAlpha = 0.0;

        if (this.services.TryGet<IContent>() == null)
        {
            this.services.AddService<IContent>(new ContentManager());
        }
        if (this.services.TryGet<IAudio>() == null)
        {
            this.services.AddService<IAudio>(new AudioService());
        }
        if (this.services.TryGet<IInput>() == null)
        {
            this.services.AddService<IInput>(new InputService());
        }

        if (this.game is Game coreGame)
        {
            IContent content = this.services.Get<IContent>();
            IAudio audio = this.services.Get<IAudio>();
            IInput input = this.services.Get<IInput>();
            coreGame.AttachServices(content, audio, input);
        }
    }

    /// <summary>Initializes the game and loads its content.</summary>
    public async Task InitializeAndLoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            this.game.Initialize();
            await this.game.LoadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to initialize or load the game.");
            throw;
        }
    }

    /// <summary>Advances the simulation with a fixed delta time (seconds).</summary>
    public void FixedUpdate(double fixedDeltaSeconds)
    {
        try
        {
            this.time.Advance(fixedDeltaSeconds, this.InterpolationAlpha);
            this.game.Update(fixedDeltaSeconds);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Update failed.");
            throw;
        }
    }

    /// <summary>Renders the current frame.</summary>
    public void Draw(IDrawContext context)
    {
        try
        {
            this.game.Draw(context);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Draw failed.");
            throw;
        }
    }

    /// <summary>Resolves a registered service.</summary>
    public TService GetService<TService>() where TService : class
    {
        return this.services.Get<TService>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            IContent? content = this.services.TryGet<IContent>();
            content?.Dispose();

            IAudio? audio = this.services.TryGet<IAudio>();
            audio?.Dispose();
        }
        catch (Exception)
        {
        }
    }
}

