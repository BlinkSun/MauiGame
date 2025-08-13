using MauiGame.Core.Contracts;
using MauiGame.Core.Scenes;
using MauiGame.Core.Time;
using MauiGame.Maui.GameView;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Numerics;

namespace SampleGame.Game.Scenes;

/// <summary>
/// Very simple title scene: shows text, waits for Space/Tap to start gameplay.
/// </summary>
public sealed partial class TitleScene(ILogger<TitleScene>? logger = null) : Scene("Title")
{
    private readonly ILogger<TitleScene> logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TitleScene>.Instance;

    private MauiGame.Core.Contracts.IFont? font;
    private IAudioClip? click;
    private float blinkTimer = 0.0f;

    /// <inheritdoc/>
    public override async Task LoadAsync(CancellationToken cancellationToken)
    {
        await base.LoadAsync(cancellationToken).ConfigureAwait(false);
        this.font = await this.Content.LoadFontAsync("Fonts/OpenSans-Regular.ttf", cancellationToken).ConfigureAwait(false);
        this.click = await this.Audio.LoadClipAsync("Audio/click.wav", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override void Update(GameTime time)
    {
        base.Update(time);
        this.blinkTimer += (float)time.DeltaSeconds;

        KeyboardState ks = this.Input.GetKeyboardState();
        TouchState ts = this.Input.GetTouchState();

        bool proceed = ks.IsDown(Key.Space) || ks.IsDown(Key.Enter) || ts.Touches.Count > 0;
        if (proceed)
        {
            try
            {
                if (this.click != null)
                {
                    IAudioInstance instance = this.Audio.Play(this.click, 1.0f, false, true);
                    double duration = this.click.DurationSeconds ?? 1.0;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(duration)).ConfigureAwait(false);
                            instance.Dispose();
                        }
                        catch (Exception disposeEx)
                        {
                            this.logger.LogError(disposeEx, "Failed to dispose click sound.");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to play click sound.");
            }

            // Signal to switch scene. Since IScene doesn't manage switching,
            // the MyGame class will replace the scene. Easiest is to raise an event or use a callback.
            this.OnStartRequested?.Invoke();
        }
    }

    /// <inheritdoc/>
    public override void Draw(IDrawContext context)
    {
        base.Draw(context);

        SkiaDrawContext sk = (SkiaDrawContext)context;
        using SkiaRenderer2D renderer = new(sk.Canvas);

        System.Numerics.Matrix3x2 camera = System.Numerics.Matrix3x2.Identity;
        renderer.Begin(camera, SKColors.Black);

        if (this.font != null)
        {
            string title = "MiniEngine + MAUI";
            string prompt = (MathF.Sin(this.blinkTimer * 5.0f) > 0.0f) ? "Tap or press Space to Start" : string.Empty;

            renderer.DrawText(this.font, title, new Vector2(40, 80), 36.0f, 0.0f);
            renderer.DrawText(this.font, prompt, new Vector2(40, 140), 20.0f, 0.0f);
        }

        renderer.End();
    }

    /// <summary>Raised when player wants to start the game.</summary>
    public event Action? OnStartRequested;
}