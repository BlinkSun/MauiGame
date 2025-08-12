using MauiGame.Core.Contracts;
using MauiGame.Core.Scenes;
using MauiGame.Core.Time;
using MauiGame.Maui.GameView;
using SkiaSharp;
using System.Numerics;

namespace SampleGame.Game.Scenes;

/// <summary>
/// Very simple title scene: shows text, waits for Space/Tap to start gameplay.
/// </summary>
public sealed partial class TitleScene(IContent content, IAudio audio, IInput input) : Scene("Title")
{
    private readonly IContent content = content ?? throw new ArgumentNullException(nameof(content));
    private readonly IAudio audio = audio ?? throw new ArgumentNullException(nameof(audio));
    private readonly IInput input = input ?? throw new ArgumentNullException(nameof(input));

    private MauiGame.Core.Contracts.IFont? font;
    private IAudioClip? click;
    private float blinkTimer = 0.0f;

    /// <inheritdoc/>
    public override async Task LoadAsync(CancellationToken cancellationToken)
    {
        await base.LoadAsync(cancellationToken).ConfigureAwait(false);
        this.font = await this.content.LoadFontAsync("Fonts/OpenSans-Regular.ttf", cancellationToken).ConfigureAwait(false);
        this.click = await this.audio.LoadClipAsync("Audio/click.wav", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override void Update(GameTime time)
    {
        base.Update(time);
        this.blinkTimer += (float)time.DeltaSeconds;

        KeyboardState ks = this.input.GetKeyboardState();
        TouchState ts = this.input.GetTouchState();

        bool proceed = ks.IsDown(Key.Space) || ks.IsDown(Key.Enter) || ts.Touches.Count > 0;
        if (proceed)
        {
            try
            {
                if (this.click != null)
                {
                    IAudioInstance instance = this.audio.Play(this.click, 1.0f, false, true);
                    instance.Dispose(); // fire-and-forget
                }
            }
            catch (Exception)
            {
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