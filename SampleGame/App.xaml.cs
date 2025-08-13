using MauiGame.Maui.GameView;
using System;

namespace SampleGame;

public partial class App : Application
{
    private readonly GamePage gamePage;

    public App(GamePage gamePage)
    {
        InitializeComponent();
        this.gamePage = gamePage ?? throw new ArgumentNullException(nameof(gamePage));
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(this.gamePage);
    }
}

