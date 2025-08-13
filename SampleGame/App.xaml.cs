using MauiGame.Maui.GameView;

namespace SampleGame;

public partial class App : Application
{
    public readonly GamePage GamePage;

    public App(GamePage gamePage)
    {
        InitializeComponent();
        GamePage = gamePage ?? throw new ArgumentNullException(nameof(gamePage));
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(GamePage);
    }
}

