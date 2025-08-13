using SampleGame.Game.Scenes;

namespace SampleGame.Game;

/// <summary>
/// The game implementation that simply sets the initial scene.
/// </summary>
public sealed class MyGame : MauiGame.Core.Game
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        Scenes.Push(new TitleScene());
    }
}
