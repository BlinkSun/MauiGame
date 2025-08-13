using MauiGame.Core.Contracts;
using MauiGame.Core.Scenes;
using MauiGame.Core.Time;
using MauiGame.Maui.GameView;
using SkiaSharp;
using System.Numerics;

namespace SampleGame.Game.Scenes;

/// <summary>
/// Basic gameplay scene: draws/moves a player sprite using keyboard/touch.
/// </summary>
public sealed partial class GameplayScene : Scene
{
    private ITexture? player;
    private MauiGame.Core.Contracts.IFont? font;
    private PlayerController? controller;
    private Vector2 position;

    public GameplayScene()
        : base("Gameplay")
    {
        this.position = new Vector2(160, 120);
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        this.controller = new PlayerController(this.Input, 220.0f);
    }

    /// <inheritdoc/>
    public override async Task LoadAsync(CancellationToken cancellationToken)
    {
        await base.LoadAsync(cancellationToken).ConfigureAwait(false);
        this.player = await this.Content.LoadTextureAsync("Assets/player.png", cancellationToken).ConfigureAwait(false);
        this.font = await this.Content.LoadFontAsync("Fonts/OpenSans-Regular.ttf", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override void Update(GameTime time)
    {
        base.Update(time);
        if (this.controller != null)
        {
            Vector2 delta = this.controller.Update(time.DeltaSeconds, this.position);
            this.position += delta;
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

        if (this.player != null)
        {
            renderer.DrawSprite(this.player, this.position, new Vector2(16, 16), new Vector2(1, 1), 0.0f, null);
        }

        if (this.font != null)
        {
            renderer.DrawText(this.font, "Move: WASD/Arrows or Touch", new Vector2(12, 24), 18.0f, 0.0f);
        }

        renderer.End();
    }
}