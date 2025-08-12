# AGENTS – Development Guidelines for MauiGame

This document outlines how contributors and maintainers should work on **MauiGame**.
It serves as an internal playbook for architecture, coding style, and long-term maintenance.

---

## 1. Project Overview

**MauiGame** is a lightweight 2D game engine built in C#/.NET 9, targeting **.NET MAUI** platforms:
- Android
- iOS
- Windows
- MacCatalyst

It is heavily inspired by **MonoGame** but designed for cross-platform MAUI UI hosting and **SkiaSharp** rendering.

The project is split into 4 parts:
1. **MauiGame.Core** → engine core contracts, math, scene system, no platform dependencies.
2. **MauiGame.Maui** → platform integration (MAUI, SkiaSharp, Input, Audio, Content loading).
3. **MauiGame.Content** → (optional) content pipeline CLI for packing textures, fonts, and maps.
4. **SampleGame** → reference implementation showcasing engine features.

---

## 2. Principles

- **Cross-platform first** → All core code must be platform-agnostic.
- **Explicit typing** → Avoid `var` unless type is obvious from right-hand side (simple `new`).
- **XML documentation** → All public members must have XML doc comments in **English**.
- **Clean architecture** → No direct MAUI code in `MauiGame.Core`.
- **Separation of concerns** → Renderer, Input, Audio, Content are independent services.

---

## 3. Code Style Rules

- **Naming**:
  - Interfaces: `IName` (e.g., `IGame`, `IRenderer2D`).
  - Classes: PascalCase (e.g., `GameHost`, `ContentManager`).
  - Private fields: no leading underscore.
  - Constants: `PascalCase`.
- **Error handling**:
  - Use `try-catch` where external resources are accessed (I/O, graphics, audio).
  - Never swallow exceptions silently — log them using `ILogger`.
- **Async/Await**:
  - Always prefer `async` APIs for I/O.
  - Avoid blocking calls (`.Result`, `.Wait()`).

---

## 4. Folder Structure Policy

- **Core**:
  - `Contracts/` → interfaces only.
  - `Math/` → math structures (if not using `System.Numerics`).
  - `Scenes/` → scene and game object management.
  - `Utilities/` → misc helpers (FPS counter, disposables, etc.).

- **Maui**:
  - `GameView/` → `GameView`, `SkiaRenderer2D`, `SkiaDrawContext`.
  - `Content/` → content loading and caching.
  - `Input/` → input state tracking.
  - `Audio/` → audio playback.
  - `Hosting/` → `GameHost` and service registry.

- **SampleGame**:
  - `Game/` → game-specific code.
  - `Resources/Raw/Assets` → textures, spritesheets, tilemaps.
  - `Resources/Raw/Audio` → music, sound effects.

---

## 5. Workflow

- **Branching**:
  - `main` → always stable and releasable.
  - `codex` → integration branch.

- **Commits**:
  - Follow [Conventional Commits](https://www.conventionalcommits.org/):
    - `feat:` for new features.
    - `fix:` for bug fixes.
    - `docs:` for documentation.
    - `refactor:` for non-breaking code changes.
    - `perf:` for performance improvements.

- **PR Review**:
  - At least one approving review required.

---

## 6. Roadmap

- ✅ Basic rendering (sprites, text)
- ✅ Content loading (textures, fonts)
- ✅ Fixed timestep loop
- ✅ Input handling (multi-touch, keyboard)
- ✅ Audio playback
- ✅ Scene manager
- ⬜ Animation system (spritesheets)
- ⬜ Tilemap loader (Tiled JSON/TMX)
- ⬜ Particle system
- ⬜ Physics integration (Box2D/Chipmunk or custom)

---

## 7. Development & Testing

- Use the .NET 9 SDK.
- Ensure the solution builds with `dotnet build`.
- Run the `SampleGame` project for manual testing; automated tests are not yet available.

---

**End of AGENTS.md**
