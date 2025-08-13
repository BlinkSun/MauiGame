using MauiGame.Core.Contracts;
using SampleGame.Game.Scenes;

namespace SampleGame.Game;

/// <summary>
/// The game implementation that manages scenes and shared assets.
/// </summary>
public sealed class MyGame : MauiGame.Core.Game
{
    private IAudioClip? bgm;
    private IAudioInstance? bgmInstance;

    /// <inheritdoc/>
    public override void Initialize()
    {
        TitleScene title = new();
        title.OnStartRequested += async () =>
        {
            try { await StartGameplayAsync(CancellationToken.None).ConfigureAwait(false); } catch (Exception) { }
        };
        Scenes.Push(title);
        
    }

    /// <inheritdoc/>
    public override async Task LoadAsync(CancellationToken cancellationToken)
    {
        bgm = await Audio.LoadClipAsync("Audio/bgm_loop.mp3", cancellationToken).ConfigureAwait(false);
        await base.LoadAsync(cancellationToken).ConfigureAwait(false);

        if (bgm != null)
        {
            bgmInstance = Audio.Play(bgm, volume: 0.5f, loop: true, autoStart: true);
        }
    }

    /// <summary>Transitions from title to gameplay.</summary>
    public async Task StartGameplayAsync(CancellationToken cancellationToken)
    {
        GameplayScene gameplay = new();
        Scenes.Replace(gameplay);
        await Scenes.EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);
    }
}

