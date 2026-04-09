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
#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
#endif

namespace MauiGame.Maui.GameView;

/// <summary>
/// A MAUI ContentPage that hosts the game loop and a SkiaSharp SKGLView for rendering.
/// </summary>
public sealed partial class GamePage : ContentPage, IDisposable
{
#if WINDOWS
    private readonly SKCanvasView view;
    private FrameworkElement? windowsPlatformView;
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
        this.view = new SKCanvasView
        {
            EnableTouchEvents = true
        };
        this.view.PaintSurface += (s, e) =>
        {
            try
            {
                SkiaDrawContext ctx = new(e.Info.Width, e.Info.Height, e.Surface.Canvas);
                this.host?.Draw(ctx);
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
#if WINDOWS
            EnsureWindowsInputHooks();
#endif
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
#if WINDOWS
        this.view.HandlerChanged -= OnViewHandlerChanged;
        UnhookWindowsPlatformEvents();
#endif
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

#if WINDOWS
    private void EnsureWindowsInputHooks()
    {
        if (this.view.Handler?.PlatformView is FrameworkElement platformView)
        {
            HookWindowsPlatformEvents(platformView);
            return;
        }

        this.view.HandlerChanged -= OnViewHandlerChanged;
        this.view.HandlerChanged += OnViewHandlerChanged;
    }

    private void OnViewHandlerChanged(object? sender, EventArgs e)
    {
        if (this.view.Handler?.PlatformView is FrameworkElement platformView)
        {
            HookWindowsPlatformEvents(platformView);
        }
    }

    private void HookWindowsPlatformEvents(FrameworkElement platformView)
    {
        if (ReferenceEquals(this.windowsPlatformView, platformView))
        {
            return;
        }

        UnhookWindowsPlatformEvents();

        this.windowsPlatformView = platformView;
        this.windowsPlatformView.KeyDown += OnWindowsKeyDown;
        this.windowsPlatformView.KeyUp += OnWindowsKeyUp;
        this.windowsPlatformView.IsTabStop = true;
        this.windowsPlatformView.Focus(FocusState.Programmatic);
    }

    private void UnhookWindowsPlatformEvents()
    {
        if (this.windowsPlatformView == null)
        {
            return;
        }

        this.windowsPlatformView.KeyDown -= OnWindowsKeyDown;
        this.windowsPlatformView.KeyUp -= OnWindowsKeyUp;
        this.windowsPlatformView = null;
    }

    private void OnWindowsKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (TryMapVirtualKey(e.Key, out KeyCodes key))
        {
            this.input.HandleKeyDown(key);
        }
    }

    private void OnWindowsKeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (TryMapVirtualKey(e.Key, out KeyCodes key))
        {
            this.input.HandleKeyUp(key);
        }
    }

    private static bool TryMapVirtualKey(VirtualKey key, out KeyCodes mapped)
    {
        mapped = key switch
        {
            VirtualKey.A => KeyCodes.A,
            VirtualKey.B => KeyCodes.B,
            VirtualKey.C => KeyCodes.C,
            VirtualKey.D => KeyCodes.D,
            VirtualKey.E => KeyCodes.E,
            VirtualKey.F => KeyCodes.F,
            VirtualKey.G => KeyCodes.G,
            VirtualKey.H => KeyCodes.H,
            VirtualKey.I => KeyCodes.I,
            VirtualKey.J => KeyCodes.J,
            VirtualKey.K => KeyCodes.K,
            VirtualKey.L => KeyCodes.L,
            VirtualKey.M => KeyCodes.M,
            VirtualKey.N => KeyCodes.N,
            VirtualKey.O => KeyCodes.O,
            VirtualKey.P => KeyCodes.P,
            VirtualKey.Q => KeyCodes.Q,
            VirtualKey.R => KeyCodes.R,
            VirtualKey.S => KeyCodes.S,
            VirtualKey.T => KeyCodes.T,
            VirtualKey.U => KeyCodes.U,
            VirtualKey.V => KeyCodes.V,
            VirtualKey.W => KeyCodes.W,
            VirtualKey.X => KeyCodes.X,
            VirtualKey.Y => KeyCodes.Y,
            VirtualKey.Z => KeyCodes.Z,
            VirtualKey.Number0 => KeyCodes.D0,
            VirtualKey.Number1 => KeyCodes.D1,
            VirtualKey.Number2 => KeyCodes.D2,
            VirtualKey.Number3 => KeyCodes.D3,
            VirtualKey.Number4 => KeyCodes.D4,
            VirtualKey.Number5 => KeyCodes.D5,
            VirtualKey.Number6 => KeyCodes.D6,
            VirtualKey.Number7 => KeyCodes.D7,
            VirtualKey.Number8 => KeyCodes.D8,
            VirtualKey.Number9 => KeyCodes.D9,
            VirtualKey.Left => KeyCodes.Left,
            VirtualKey.Right => KeyCodes.Right,
            VirtualKey.Up => KeyCodes.Up,
            VirtualKey.Down => KeyCodes.Down,
            VirtualKey.Space => KeyCodes.Space,
            VirtualKey.Enter => KeyCodes.Enter,
            VirtualKey.Escape => KeyCodes.Escape,
            VirtualKey.Shift => KeyCodes.Shift,
            VirtualKey.Control => KeyCodes.Control,
            VirtualKey.Menu => KeyCodes.Alt,
            VirtualKey.Tab => KeyCodes.Tab,
            _ => KeyCodes.Unknown
        };

        return mapped != KeyCodes.Unknown;
    }
#endif
}
