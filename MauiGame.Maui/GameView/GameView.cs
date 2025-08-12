using MauiGame.Core.Contracts;
using MauiGame.Maui.Hosting;
using MauiGame.Maui.Input;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;

namespace MauiGame.Maui.GameView;

/// <summary>
/// A MAUI ContentView that hosts the game loop and a SkiaSharp SKGLView for rendering.
/// </summary>
public sealed partial class GameView : ContentView
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

    private double accumulatorSeconds;
    private long lastTicks;
    private bool isRunning;

    /// <summary>Create a new GameView driven at a target FPS (default 60).</summary>
    public GameView(IGame game, ServiceRegistry? services = null, double targetFps = 60.0)
    {
        ArgumentNullException.ThrowIfNull(game);

        this.fixedDeltaSeconds = 1.0 / targetFps;
        this.stopwatch = new Stopwatch();
#if WINDOWS
        this.view = new SKCanvasView();
        this.view.PaintSurface += (s, e) =>
        {
            try
            {
                SkiaDrawContext ctx = new(e.Info.Width, e.Info.Height, e.Surface.Canvas);
                this.host.Draw(ctx);
            }
            catch { }
        };
#else
        this.view = new SKGLView { EnableTouchEvents = true, HasRenderLoop = false };
        this.view.PaintSurface += OnPaintSurface;
#endif

        // Services and host
        this.input = new InputService();
        ServiceRegistry registry = services ?? new ServiceRegistry();
        registry.TryAddService<IInput>(this.input);

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

        this.Loaded += async (s, e) => await StartAsync().ConfigureAwait(false);
        this.Unloaded += (s, e) => Stop();
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
            System.Diagnostics.Debug.WriteLine(ex);
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
        catch (Exception)
        {
        }
    }

    private void StartTimer()
    {
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
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
                System.Diagnostics.Debug.WriteLine(ex);
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
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private void OnTouch(object? sender, SkiaSharp.Views.Maui.SKTouchEventArgs e)
    {
        try
        {
            this.input.HandleTouch(e);
            e.Handled = true;
        }
        catch (Exception)
        {
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
        try { this.input.HandleKeyDown(key); } catch (Exception) { }
    }

    /// <summary>
    /// Forwards a key-up event (call from your Page/Window if you have platform key hooks).
    /// </summary>
    public void ForwardKeyUp(KeyCodes key)
    {
        try { this.input.HandleKeyUp(key); } catch (Exception) { }
    }
}