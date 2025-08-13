using MauiGame.Core.Contracts;
using MauiGame.Maui.GameView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System;

namespace MauiGame.Maui.Hosting;

/// <summary>
/// Extension methods to wire up the MAUI game engine.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>Adds the MAUI game engine and registers the game and page.</summary>
    /// <typeparam name="TGame">Type implementing the game.</typeparam>
    /// <param name="builder">Application builder.</param>
    /// <param name="configure">Optional callback to configure engine options.</param>
    public static MauiAppBuilder UseMauiGame<TGame>(this MauiAppBuilder builder, Action<MauiGameOptions>? configure = null)
        where TGame : class, IGame
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        MauiGameOptions options = new();
        configure?.Invoke(options);

        builder.UseSkiaSharp();

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<Func<IContent, IAudio, IInput, IGame>>(sp => (content, audio, input) =>
            ActivatorUtilities.CreateInstance<TGame>(sp, content, audio, input));
        builder.Services.AddSingleton<GamePage>();

        return builder;
    }
}

