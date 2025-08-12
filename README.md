# MauiGame – Lightweight 2D Game Engine for .NET MAUI

**MauiGame** is a cross-platform 2D game engine for **.NET MAUI**, inspired by MonoGame but designed for modern cross-platform apps with **SkiaSharp** rendering.

It runs on:
- Android
- iOS
- Windows
- MacCatalyst

---

## ✨ Features

- 🎮 **Game loop** with fixed timestep update & variable framerate rendering.
- 🖌 **2D Renderer** powered by [SkiaSharp](https://github.com/mono/SkiaSharp).
- 📦 **Content manager** for textures, fonts, and audio.
- 🎧 **Audio support** via [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio).
- 🖱 **Input system** supporting touch, mouse, and keyboard.
- 🗂 **Scene management** for easy game state transitions.
- 📜 **Cross-platform**: single codebase, runs everywhere MAUI runs.

---

## 🗺 Roadmap

- [x] Basic rendering (sprites, text)
- [x] Content loading (textures, fonts)
- [x] Fixed timestep loop
- [x] Input handling (multi-touch, keyboard)
- [x] Audio playback
- [x] Scene manager
- [ ] Animation system (spritesheets)
- [ ] Tilemap loader (Tiled JSON/TMX)
- [ ] Particle system
- [ ] Physics integration (Box2D/Chipmunk or custom)

---

## 📦 Solution Structure

```text
MauiGame.sln
├─ MauiGame.Core     # Core engine contracts & game logic
├─ MauiGame.Maui     # MAUI integration (SkiaSharp, Input, Audio)
├─ MauiGame.Content  # Optional CLI content pipeline
└─ SampleGame        # Example game using MauiGame
```

---

## 🚀 Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/BlinkSun/MauiGame.git
cd MauiGame
```

### 2. Open the solution
Open `MauiGame.sln` in **Visual Studio 2022** or **Rider** with .NET 9 SDK installed.

### 3. Run the sample game
Set **SampleGame** as the startup project and run it on your preferred platform (Android, Windows, iOS, or MacCatalyst).

---

## 📂 Directory Overview

### MauiGame.Core
- Contracts (`IGame`, `IRenderer2D`, etc.)
- Scene system
- Math utilities
- Cross-platform logic

### MauiGame.Maui
- `GameView` hosting SkiaSharp canvas
- Skia renderer implementation
- Input handling (touch, keyboard, mouse)
- Audio service
- Content manager

### MauiGame.Content (optional)
- CLI tool for packing textures and fonts
- Atlas generation
- Tiled map importer

### SampleGame
- Demonstrates basic rendering, input, and scenes.
- Contains game assets (textures, audio).

---

## 🛠 Example Usage

```csharp
public sealed class MyGame : IGame
{
    private ITexture player;
    private Vector2 position;

    public void Initialize()
    {
        position = new Vector2(100, 100);
    }

    public async Task LoadAsync(CancellationToken token)
    {
        var content = new ContentManager();
        player = await content.LoadTextureAsync("Assets/player.png", token);
    }

    public void Update(double delta)
    {
        position.X += 50f * (float)delta;
    }

    public void Draw(IDrawContext context)
    {
        var skCtx = (SkiaDrawContext)context;
        using var renderer = new SkiaRenderer2D(skCtx.Canvas);
        renderer.Begin(Matrix3x2.Identity, SkiaSharp.SKColors.Black);
        renderer.DrawSprite(player, position, new Vector2(16, 16), Vector2.One, 0f, null);
        renderer.End();
    }
}
```

---

## 🧩 Dependencies

- [.NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- [SkiaSharp](https://github.com/mono/SkiaSharp)
- [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio)

---

## 🤝 Contributing

Contributions are welcome!  
See [AGENTS.md](AGENTS.md) for internal guidelines and development workflow.

---

## 📜 License

MIT License. See [LICENSE](LICENSE) for details.
