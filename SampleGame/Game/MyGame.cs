using MauiGame.Core;
using MauiGame.Core.Contracts;
using SampleGame.Game.Scenes;

namespace SampleGame.Game;

/// <summary>
/// The game implementation that manages scenes and shared assets.
/// </summary>
public sealed class MyGame : Game
{
    private IAudioClip? bgm;
    private IAudioInstance? bgmInstance;

    /// <inheritdoc/>
    public override void Initialize()
    {
        TitleScene title = new(this.Content, this.Audio, this.Input);
        title.OnStartRequested += async () =>
        {
            try { await StartGameplayAsync(CancellationToken.None).ConfigureAwait(false); } catch (Exception) { }
        };
        this.Scenes.Push(title);
    }

    /// <inheritdoc/>
    public override async Task LoadAsync(CancellationToken cancellationToken)
    {
        this.bgm = await this.Audio.LoadClipAsync("Audio/bgm_loop.mp3", cancellationToken).ConfigureAwait(false);
        await base.LoadAsync(cancellationToken).ConfigureAwait(false);

        if (this.bgm != null)
        {
            this.bgmInstance = this.Audio.Play(this.bgm, volume: 0.5f, loop: true, autoStart: true);
        }
    }

    /// <summary>Transitions from title to gameplay.</summary>
    public async Task StartGameplayAsync(CancellationToken cancellationToken)
    {
        GameplayScene gameplay = new(this.Content, this.Audio, this.Input);
        this.Scenes.Replace(gameplay);
        await this.Scenes.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
    }
}

