using MauiGame.Core.Contracts;
using MauiGame.Maui.Audio;
using MauiGame.Maui.Content;
using MauiGame.Maui.GameView;
using MauiGame.Maui.Hosting;
using MauiGame.Maui.Input;
using Microsoft.Extensions.Logging;
using SampleGame.Game;

namespace SampleGame;

/// <summary>
/// Main page that hosts the GameView.
/// </summary>
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Build a registry so we can inject the same services into the game instance.
        ServiceRegistry registry = new();

        InputService input = new();
        ContentManager content = new();
        AudioService audio = new();

        registry.AddService<IInput>(input);
        registry.AddService<IContent>(content);
        registry.AddService<IAudio>(audio);

        IGame game = new MyGame(content, audio, input);

        ILogger<GameView> logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<GameView>();
        GameView view = new(game, logger, registry, 60.0);
        Content = view;
    }
}
