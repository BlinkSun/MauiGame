using MauiGame.Core.Contracts;
using MauiGame.Core.Scenes;
using MauiGame.Core.Time;
using SampleGame.Game.Scenes;

namespace SampleGame.Game;
/// <summary>
/// The game implementation that manages scenes and shared assets.
/// </summary>
public sealed class MyGame(IContent content, IAudio audio, IInput input) : IGame
{
    private readonly IContent content = content ?? throw new ArgumentNullException(nameof(content));
    private readonly IAudio audio = audio ?? throw new ArgumentNullException(nameof(audio));
    private readonly IInput input = input ?? throw new ArgumentNullException(nameof(input));
    private readonly SceneManager scenes = new(logger: Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
    private IAudioClip? bgm;
    private IAudioInstance? bgmInstance;
    private readonly GameTime time = new();

    /// <inheritdoc/>
    public void Initialize()
    {
        // Start on the title scene
        TitleScene title = new(this.content, this.audio, this.input);
        title.OnStartRequested += async () =>
        {
            try { await StartGameplayAsync(CancellationToken.None).ConfigureAwait(false); } catch (Exception) { }
        };
        this.scenes.Push(title);
    }

    /// <inheritdoc/>
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        // Preload global audio (bgm).
        this.bgm = await this.audio.LoadClipAsync("Audio/bgm_loop.mp3", cancellationToken).ConfigureAwait(false);

        // Ensure current scene is loaded.
        await this.scenes.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);

        // Start BGM.
        if (this.bgm != null)
        {
            this.bgmInstance = this.audio.Play(this.bgm, volume: 0.5f, loop: true, autoStart: true);
        }
    }

    /// <inheritdoc/>
    public void Update(double deltaSeconds)
    {
        this.time.Advance(deltaSeconds, 0.0);
        this.scenes.Update(this.time);
    }

    /// <inheritdoc/>
    public void Draw(IDrawContext context)
    {
        this.scenes.Draw(context);
    }

    /// <summary>Transitions from title to gameplay.</summary>
    public async Task StartGameplayAsync(CancellationToken cancellationToken)
    {
        GameplayScene gameplay = new(this.content, this.audio, this.input);
        this.scenes.Replace(gameplay);
        await this.scenes.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
    }
}