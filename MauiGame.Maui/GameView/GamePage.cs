using MauiGame.Core.Contracts;
using MauiGame.Maui.Audio;
using MauiGame.Maui.Content;
using MauiGame.Maui.Hosting;
using MauiGame.Maui.Input;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MauiGame.Maui.GameView;

/// <summary>
/// A MAUI ContentPage that hosts the game loop and a SkiaSharp SKGLView for rendering.
/// </summary>
public sealed partial class GamePage : ContentPage, IDisposable
{
#if WINDOWS
    private readonly SKCanvasView view;
#else
    private readonly SKGLView view;
#endif
    private readonly Stopwatch stopwatch;
    private readonly double fixedDeltaSeconds;
    private readonly GameHost host;
    private readonly InputService input;
    private readonly ILogger<GamePage> logger;

    private double accumulatorSeconds;
    private long lastTicks;
    private bool isRunning;
    private bool disposed;

    /// <summary>Create a new GamePage driven at the configured target FPS.</summary>
    /// <param name="game">Game instance to run.</param>
    /// <param name="logger">Logger used for reporting errors and warnings.</param>
    /// <param name="options">Engine configuration options.</param>
    public GamePage(
        IGame game,
        ILogger<GamePage> logger,
        MauiGameOptions options)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        this.fixedDeltaSeconds = 1.0 / options.TargetFps;
        this.stopwatch = new Stopwatch();
        this.logger = logger;
#if WINDOWS
        this.view = new SKCanvasView();
        this.view.PaintSurface += (s, e) =>
        {
            try
            {
                SkiaDrawContext ctx = new(e.Info.Width, e.Info.Height, e.Surface.Canvas);
                this.host.Draw(ctx);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during paint surface.");
            }
        };
#else
        this.view = new SKGLView { EnableTouchEvents = true, HasRenderLoop = false };
        this.view.PaintSurface += OnPaintSurface;
#endif

        // Services and host
        this.input = new InputService();
        ContentManager content = new();
        AudioService audio = new();
        ServiceRegistry registry = new();
        registry.AddService<IInput>(this.input);
        registry.AddService<IContent>(content);
        registry.AddService<IAudio>(audio);

        this.host = new GameHost(game, registry);

        // Wire up input
        this.view.Touch += OnTouch;
#if WINDOWS || MACCATALYST
        //this.view.MouseMove += OnMouseMove;
        //this.view.MouseDown += OnMouseDown;
        //this.view.MouseUp += OnMouseUp;
#endif
        // Keyboard is platform-specific; expose public methods for the page/window to forward events if available.
        this.Content = this.view;

        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    /// <summary>Starts the loop and loads the game.</summary>
    private async Task StartAsync()
    {
        try
        {
            if (this.isRunning)
            {
                return;
            }

            await this.host.InitializeAndLoadAsync(CancellationToken.None).ConfigureAwait(false);

            this.isRunning = true;
            this.stopwatch.Start();
            this.lastTicks = this.stopwatch.ElapsedTicks;
            StartTimer();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to start GamePage.");
            throw;
        }
    }

    /// <summary>Stops the loop.</summary>
    private void Stop()
    {
        this.isRunning = false;
        try
        {
            this.stopwatch.Stop();
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Error while stopping stopwatch.");
        }
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            await StartAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to load GamePage.");
        }
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        if (this.isRunning)
        {
            Stop();
        }

        this.view.Touch -= OnTouch;
        this.Loaded -= OnLoaded;
        this.Unloaded -= OnUnloaded;

        this.host.Dispose();
        GC.SuppressFinalize(this);
    }

    private void StartTimer()
    {
        Dispatcher.StartTimer(TimeSpan.FromSeconds(this.fixedDeltaSeconds), () =>
        {
            try
            {
                if (!this.isRunning) return false;

                long ticks = this.stopwatch.ElapsedTicks;
                long deltaTicks = ticks - this.lastTicks;
                this.lastTicks = ticks;

                double deltaSeconds = (double)deltaTicks / Stopwatch.Frequency;
                this.accumulatorSeconds += deltaSeconds;

                int safety = 0;
                while (this.accumulatorSeconds >= this.fixedDeltaSeconds && safety < 5)
                {
                    this.host.FixedUpdate(this.fixedDeltaSeconds);
                    this.accumulatorSeconds -= this.fixedDeltaSeconds;
                    safety++;
                }

                double alpha = this.accumulatorSeconds / this.fixedDeltaSeconds;
                this.host.InterpolationAlpha = alpha;

                this.view.InvalidateSurface();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unhandled exception in game loop iteration.");
                return false;
            }

            return true;
        });
    }

    private void OnPaintSurface(object? sender, SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs e)
    {
        try
        {
            SkiaDrawContext context = new(e.BackendRenderTarget.Width, e.BackendRenderTarget.Height, e.Surface.Canvas);
            this.host.Draw(context);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during paint surface.");
        }
    }

    private void OnTouch(object? sender, SkiaSharp.Views.Maui.SKTouchEventArgs e)
    {
        try
        {
            this.input.HandleTouch(e);
            e.Handled = true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error handling touch event.");
            e.Handled = false;
        }
    }

#if WINDOWS || MACCATALYST
    //private void OnMouseMove(object? sender, SkiaSharp.Views.Maui.SKMouseEventArgs e)
    //{
    //    try
    //    {
    //        this.input.HandleMouseMove((float)e.Location.X, (float)e.Location.Y);
    //    }
    //    catch (Exception)
    //    {
    //    }
    //}

    //private void OnMouseDown(object? sender, SkiaSharp.Views.Maui.SKMouseEventArgs e)
    //{
    //    try
    //    {
    //        this.input.HandleMouseDown(e.Buttons);
    //    }
    //    catch (Exception)
    //    {
    //    }
    //}

    //private void OnMouseUp(object? sender, SkiaSharp.Views.Maui.SKMouseEventArgs e)
    //{
    //    try
    //    {
    //        this.input.HandleMouseUp(e.Buttons);
    //    }
    //    catch (Exception)
    //    {
    //    }
    //}
#endif

    /// <summary>
    /// Forwards a key-down event (call from your Page/Window if you have platform key hooks).
    /// </summary>
    public void ForwardKeyDown(KeyCodes key)
    {
        try
        {
            this.input.HandleKeyDown(key);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error forwarding key down.");
            throw;
        }
    }

    /// <summary>
    /// Forwards a key-up event (call from your Page/Window if you have platform key hooks).
    /// </summary>
    public void ForwardKeyUp(KeyCodes key)
    {
        try
        {
            this.input.HandleKeyUp(key);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error forwarding key up.");
            throw;
        }
    }
}
