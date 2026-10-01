# Redungeon — Comprehensive Architecture & Codebase Analysis (AUTODOCS)

> **Document Classification**: Engineering Architecture Audit & Modernization Blueprint  
> **Target Framework**: .NET 10.0 | **Engine**: MonoGame Framework 3.8.5.1  
> **Analyzed Projects**: `RedungeonPC.csproj` (DesktopGL) & `Redungeon.Android.csproj` (Android)  
> **Author**: Gemini 3.8 Flash, TheLanc3  
> **Date**: September 2026

---

## 1. Executive Summary

**Redungeon** is an enhanced cross-platform port of the classic Nitrome mobile procedural dungeon crawler, re-engineered on modern **.NET 10** using the **MonoGame 3.8.5.1** game engine. The codebase combines reverse-engineered legacy game logic, procedural dungeon generation, custom 2D lighting and shaders, and modern enhancements including gamepads, haptics, pixel-perfect resolution handling, dynamic lighting/shadows, and the new **Echoes** dungeon expansion mode.

The folder `RedungeonPC_ResolutionManager` currently houses both the desktop and mobile projects alongside the complete gameplay code:
1. **`RedungeonPC.csproj`**: Self-contained DesktopGL executable targeting `net10.0` (Windows and Linux).
2. **`Android/Redungeon.Android.csproj`**: Mobile application package targeting `net10.0-android` (arm64 APK).

### High-Level Architecture Assessment
* **Engine Pattern**: Hybrid Monolithic Service Locator (`Core.cs`) + Component Object Model (`Component.cs`).
* **Simulation Model**: Fixed 60-Hz discrete tick simulation (`IsFixedTimeStep = true`, `TargetElapsedTime = 1/60s`).
* **Rendering Model**: Custom multi-pass deferred 2D renderer (`Renderer.cs`) with integer-ratio pixel scaling (`ResolutionManager.cs`), CPU raycasted dynamic lighting and soft shadows, bloom, and post-processing screen shaders.
* **Input Subsystem**: Unified input abstraction (`InputManager.cs`) mapping physical keyboard, mouse, and gamepads (Xbox/PlayStation) into discrete `GameAction` events with automated device detection (`InputDeviceTracker.cs`).
* **Platform Sharing Model**: Shared-source file linking (`<Compile Include="../Knighter*/**/*.cs">`) rather than a compiled Core Class Library.

---

## 2. Project Analysis: `RedungeonPC.csproj` (Desktop Platform)

### 2.1 Project Profile & Build Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RollForward>Major</RollForward>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
    <EnableMGCBItems>false</EnableMGCBItems>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MonoGame.Framework.DesktopGL" Version="3.8.5.1" />
    <PackageReference Include="MonoGame.Content.Builder.Task" Version="3.8.5.1" />
  </ItemGroup>
  <ItemGroup>
    <Compile Remove="Tests\**\*.cs" />
    <Compile Remove="Android\**\*.cs" />
  </ItemGroup>
</Project>
```

### 2.2 Entry Point & Runtime Initialization (`Program.cs`)
* **Base Directory Locking**: Fixes the execution working directory to `AppContext.BaseDirectory` so that launching from shortcuts, launchers (Steam, itch.io), or arbitrary working directories can reliably resolve asset paths.
* **Native SDL2 Rumble Hints**: Sets environment variables `SDL_JOYSTICK_HIDAPI_PS4_RUMBLE=1` and `SDL_JOYSTICK_HIDAPI_PS5_RUMBLE=1` before SDL2 initializes, unlocking extended Bluetooth haptic feedback for DualShock 4 and DualSense gamepads.
* **Crash Resilience**: Wraps the game lifecycle in a global try/catch block that dumps an unhandled exception trace into `crash_log.txt` next to the executable.
* **QA Test Automation (`#if DEBUG`)**: Integrates `QaSession.Configure(args)` for headless execution, automated screenshot regression testing, and fixture validation.

### 2.3 Desktop Game Lifecycle & Window Management (`Knighter/MobileGame.cs`)
* **Window Modes**: Supports `Windowed`, `Fullscreen` (exclusive/borderless toggle), and `Borderless` window modes via `SetWindowMode()`.
* **Dynamic Window Resizing**: Attaches to `Window.ClientSizeChanged` with a debounce mechanism (`clientResizePending`, `pendingClientWidth`, `pendingClientHeight`) evaluated during `Update()`. This avoids tearing and race conditions on continuous window drag resizing.
* **Resolution Propagation**: Any window mutation directly notifies `ResolutionManager.Recalculate()`, recalculating pixel scale and notifying dependent subsystems (`Renderer`, `Camera`, `HUD`) via the `ResolutionManager.Changed` event.
* **Custom Software Hand Cursor**: Loads `swipe_hand_1` from the sprite sheet, scales it 2x, and registers it as an OS-level `MouseCursor.FromTexture2D()`.

### 2.4 Desktop Input Emulation (`Knighter/MouseTouchAdapter.cs`)
* **Touch Adaptation**: Converts raw mouse clicks and movements into MonoGame `TouchCollection` and `TouchLocation` events.
* **Coordinate Space Normalization**: Converts window client bounds into game logical units through `ResolutionManager.WindowToLogical()`, clipping inputs that occur within letterbox/pillarbox bars.
* **Edge Detection**: Generates `TouchLocationState.Pressed`, `Moved`, and `Released` events seamlessly so that touch-first menus work naturally with mouse clicks.

### 2.5 Strengths & Weaknesses (Desktop)
* **Strengths**:
  * Clean single-file publish configuration with native library self-extraction.
  * Robust integer pixel scaling with zero shimmer or non-uniform pixel stretching.
  * Native DualShock/DualSense vibration integration via SDL2 hints.
  * Comprehensive QA automation hooks for reproducible visual validations.
* **Weaknesses**:
  * Project file acts as the repository root container, requiring manual exclusions (`<Compile Remove="..."/>`).
  * `RedungeonPC.csproj` is configured as a self-contained `WinExe`, preventing test projects from referencing it directly (`error NETSDK1151`).
  * DesktopGL references SDL2 binaries that require post-publish workarounds (`ExtractSDL2ToRoot`).

---

## 3. Project Analysis: `Android/Redungeon.Android.csproj` (Mobile Platform)

### 3.1 Project Profile & Build Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-android</TargetFramework>
    <OutputType>Exe</OutputType>
    <ApplicationId>com.redungeon.personal</ApplicationId>
    <ApplicationVersion>5</ApplicationVersion>
    <ApplicationDisplayVersion>1.0.5</ApplicationDisplayVersion>
    <SupportedOSPlatformVersion>26</SupportedOSPlatformVersion>
    <RuntimeIdentifier>android-arm64</RuntimeIdentifier>
    <AndroidPackageFormats>apk</AndroidPackageFormats>
    <AndroidUseAssemblyStore>true</AndroidUseAssemblyStore>
    <EmbedAssembliesIntoApk>true</EmbedAssembliesIntoApk>
    <RunAOTCompilation>false</RunAOTCompilation>
    <AndroidEnableMarshalMethods>false</AndroidEnableMarshalMethods>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MonoGame.Framework.Android" Version="3.8.5.1" />
    <PackageReference Include="MonoGame.Content.Builder.Task" Version="3.8.5.1" />
    <Compile Include="*.cs" />
    <Compile Include="../Knighter*/**/*.cs" Exclude="../Knighter/MobileGame.cs;../Knighter/MouseTouchAdapter.cs" />
    <AndroidAsset Include="../Content/**/*.*" Exclude="..." Link="Content/%(RecursiveDir)%(Filename)%(Extension)" />
    <MonoGameContentReference Include="../Content/Android.mgcb" />
  </ItemGroup>
</Project>
```

### 3.2 Native Android Activity Integration (`MainActivity.cs`)
* **Lifecycle & Immersive UI**: Subclasses `AndroidGameActivity`, applying `Theme.NoTitleBar.Fullscreen` and `ScreenOrientation.Sensor`. Enforces sticky immersive mode (`SystemUiFlags.ImmersiveSticky | SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen`).
* **Haptics Bridge**: Binds `Android.OS.Vibrator` to `Knighter.Gameplay.Rumble.PhonePulse` and `PhoneStop`. Uses `VibrationEffect.CreateOneShot()` with amplitude scaling (0-255) for devices with amplitude control.
* **Asset Extraction Hack (`CopyAssets`)**:
  * Recursively extracts raw assets from the APK `AndroidAsset` directory to the internal disk storage (`FilesDir`).
  * **Critical Observation**: This copying occurs because the game engine relies on direct file system access (`File.OpenRead`, `File.ReadAllText`) rather than reading from Android asset streams via `TitleContainer.OpenStream`. This prolongs initial launch time and doubles disk usage on the device.

### 3.3 Mobile Lifecycle & Surface Orientation (`Android/MobileGame.cs`)
* **Orientation & Surface Debouncing**: Android handles sensor-driven display rotation dynamically. `MobileGame.cs` implements an 8-frame surface stability filter (`pendingStableFrames < 8`) to wait for Android surface re-allocation before triggering `ResolutionManager.Recalculate()`.
* **Viewport Re-anchoring**: Guards against intermediate zero-sized surfaces during activity pause/resume cycles.
* **Touch Handling (`Android/MouseTouchAdapter.cs`)**: Maps physical touches directly from `TouchPanel.GetState()` into the game's logical coordinates using `ResolutionManager.WindowToLogical()`.

### 3.4 Strengths & Weaknesses (Android)
* **Strengths**:
  * Up-to-date target on `.NET 10-android` (API 26+) leveraging arm64 runtime optimizations.
  * Proper Android sensor-orientation handling with surface debounce logic.
  * Modern haptics using Android's `VibrationEffect` API with amplitude control.
* **Weaknesses**:
  * **Linked Source Anti-Pattern**: Recompiles parent source trees using wildcards (`../Knighter*/**/*.cs`) with selective exclusion files.
  * **Asset Extraction Overhead**: Duplicating APK assets to `FilesDir` wastes storage and causes first-launch stutter.
  * **Duplicate Heads**: Maintains separate `MobileGame.cs` and `MouseTouchAdapter.cs` implementations instead of sharing a single game class with platform services.

---

## 4. Deep-Dive Subsystem Architecture

```
+-------------------------------------------------------------------------------+
|                               Knighter Engine                                 |
+-------------------------------------------------------------------------------+
| [Core] (Service Locator Singleton)                                           |
|   |--> [ResolutionManager] --> Pixel-Scale & Coordinate Transform             |
|   |--> [Renderer]          --> Layers, SpriteBatch, Shaders, 2D Lights, Bloom |
|   |--> [SpriteManager]     --> Texture Atlases, Dynamic Textures, Fonts       |
|   |--> [AudioManager]      --> SoundPool, Background Music Thread             |
|   |--> [InputManager]      --> Keyboard / Mouse / Gamepad Snapshot Pipeline   |
|   |--> [MessageManager]    --> Pub/Sub Message Bus with Delay Dispatch        |
|   |--> [Storage]           --> Key-Value Persistence (save.dat)               |
|   |--> [LocaleManager]     --> Multi-language Dictionary (JSON)               |
|   |--> [State Stack]       --> Finite State Machine (Play, Menu, Shop, etc.)  |
+-------------------------------------------------------------------------------+
                                     |
              +----------------------+----------------------+
              |                                             |
   [RedungeonPC.csproj]                         [Redungeon.Android.csproj]
   - DesktopGL (.NET 10)                         - Android (.NET 10)
   - SDL2 Rumble / Window Manager                - Android Activity & Vibrator
   - MouseTouchAdapter (Mouse -> Touch)          - MouseTouchAdapter (TouchPanel)
```

### 4.1 Core Engine & Component System (`Knighter/Core.cs`, `Component.cs`)
* **Pattern**: Monolithic Service Locator.
* **Role**: `Core` acts as the global hub instantiating and owning every subsystem:
  ```csharp
  public static Core Instance => instance;
  public Renderer Renderer { get; private set; }
  public ResolutionManager ResolutionManager { get; private set; }
  public InputManager Input { get; private set; }
  public SpriteManager SpriteManager { get; private set; }
  public ParticleManager ParticleManager { get; private set; }
  public AudioManager AudioManager { get; private set; }
  public MessageManager MessageManager { get; private set; }
  public Storage Storage { get; private set; }
  public Store Store { get; private set; }
  public AdsManager AdsManager { get; private set; }
  public LevelModules LevelModules { get; private set; }
  public Achievements Achievments { get; private set; }
  ```
* **Base Component (`Component.cs`)**:
  All major game classes (`Entity`, `State`, `UIWidget`, `LevelGenerator`) inherit from `Component`. It provides shorthand accessors to `Core.Instance`:
  * `R` -> `core.Renderer`
  * `_()` -> `core.SpriteManager.GetSprite()`
  * `__()` -> `core.LocaleManager.GetForCurrentLocale()`
  * `SendMessage()` -> `core.MessageManager.Send()`
  * `_rnd()`, `_sin()`, `_cos()` -> Math helpers.

### 4.2 Resolution & Rendering Pipeline (`Knighter.Graphics`)

#### ResolutionManager (`ResolutionManager.cs`)
* Solves the pixel-art scaling dilemma across high-resolution and arbitrary aspect ratio monitors:
  * Calculates an **integer scale factor** (`PixelScale = 1, 2, 3, 4, ...`) based on a target reference height (`266px / GuiScale`).
  * Computes **game logical coordinates** (`LogicalWidth`, `LogicalHeight`).
  * Computes the backbuffer destination viewport (`DestRect`), automatically centering the viewport with letterboxing or pillarboxing if the window aspect ratio differs from the virtual canvas.
  * Provides a single pre-calculated `ScaleMatrix` passed directly to `SpriteBatch.Begin()`.

#### Renderer (`Renderer.cs`)
* **Deferred Draw Queue**: Entities do not render directly to the screen; instead, calls to `Draw()`, `DrawW()`, `DrawText()` register `DrawDesc` items into a layered queue (`Dictionary<string, Layer>`).
* **Multi-Pass Compositing Pipeline**:
  1. **World Pass**: Renders floor tiles, walls, entities, items, and particles in world space transformed by `Camera.Position` and `Camera.Zoom`.
  2. **Dynamic 2D Lighting & Shadow Pass (`LightManager.cs`, `LightBloom.cs`)**:
     * Calculates up to 24 active dynamic point lights.
     * Computes CPU-based raycast occlusion against obstacle boundaries to render soft shadows.
     * Generates a separate light map texture buffer, blends it onto the scene using custom blend states (`ScreenBlend`, `EraseAlphaBlend`), and applies optional bloom halos.
  3. **Post-Processing Shaders (`Shaders/`)**:
     * Applies `DefaultShader.fx`, `OverlayShader.fx` (purple creeping mist/smoke), and `PostEffectShader.fx` (spotlight, ice frost borders, poison tint).
  4. **UI & HUD Pass**: Renders screen-space user interface elements, dialogs, coin counters, and cursor graphics unconstrained by world camera zoom.

### 4.3 Input System (`Knighter.Input`)
* **Architecture**: Input Snapshot Pipeline.
* **`IInputSource` / `InputSnapshot`**: Captures physical `KeyboardState`, `MouseState`, and `GamePadState` once per tick into an immutable `InputSnapshot`. This design permits deterministic input injection for automated tests and replay playback (`QaSession`).
* **Logical Action Mapping (`GameAction.cs`)**:
  Maps raw keys and controller buttons into high-level game actions:
  * Movement: `MoveUp`, `MoveDown`, `MoveLeft`, `MoveRight` (WASD, Arrow Keys, D-Pad, Left Analog Stick).
  * Actions: `Action`, `Back`, `Pause`, `Ability` (Space, Enter, Escape, Controller A/B/Start).
* **Device Tracker (`InputDeviceTracker.cs`)**: Automatically identifies the active controller type (`KeyboardMouse`, `Xbox`, `PlayStation`, `GenericGamepad`) based on the most recent button press, allowing dynamic prompt switching in the UI.

### 4.4 Entities & Gameplay Architecture (`Knighter.Entities`, `Knighter.Gameplay`)
* **Grid-Based Discrete Movement**: The game runs on a 16x16 grid coordinate system. Entities translate between discrete tile coordinates using `FloatBox` / `TweenBox` smooth interpolation.
* **Entity Hierarchy**:
  * Base: `Entity : Component` -> implements velocity, flight, sliding, platforms, age, and state machines.
  * Characters: `Character` -> `PlayerEntity` -> 13 distinct playable characters (`KnightChar`, `Lykos` / `WolfChar`, `VampireChar`, `MedusaChar`, `NathanChar`, `PanicBotChar`, `BraggChar`, etc.).
  * Obstacles & Traps: `SpikesEntity`, `PusherEntity`, `SawEntity`, `RotobladeEntity`, `ZapperEntity`, `FirewallEntity`.
  * Enemies: `BatEntity`, `SlimeEntity`, `SpiderEntity`, `SerpentEntity`, `GhostEntity`, `WispEntity`.
* **Dungeon Generation (`LevelGenerator.cs`)**:
  * Constructs an infinite vertical dungeon procedurally by stitching pre-authored JSON room modules (`modules.json`).
  * Manages corridor chunks, branch connections, tile type conversions (Floor, Wall, Pit, Fragile, Ice), and entity spawns.
  * Spawns the "Terminator" (the Grue creeping darkness) that rises from the bottom of the map, punishing indecision.

### 4.5 UI & Menu Architecture (`Knighter.UI`)
* **Focus & Navigation Controller (`MenuFocusController.cs`, `MenuInputRouter.cs`)**:
  * Unifies mouse, keyboard, and gamepad input into a unified widget navigation system.
  * Supports auto-scroll, focus indices, press/release animations, and sound triggers.
* **Widget Hierarchy (`IMenuWidget`)**:
  * `MenuButtonWidget`, `MenuToggleWidget`, `MenuSliderWidget`, `MenuValueSelectorWidget`, `MenuPageWidget`, `AchievementBrowserWidget`, `StatisticsBrowserWidget`.
* **Pixel Button Rendering (`PixelButtonPainter.cs`)**:
  * Procedurally slices and renders 9-patch pixel art buttons, borders, and text labels without asset stretching.

### 4.6 Pub/Sub Messaging System (`Knighter.Messages`)
* **`MessageManager.cs`**:
  * Decouples systems via strongly-typed messages (`SpawnEntityMessage`, `PlaySoundMessage`, `ButtonTriggerMessage`, `PushStateMessage`, `ScreenshotMessage`).
  * Features delayed message delivery: messages can specify a `delay` (in ticks) to trigger actions in future frames without spawning separate timer coroutines.

### 4.7 Testing & QA Architecture (`Tests/`, `Knighter.QA`)
* **`RedungeonPC.EchoesTests`**:
  * Standalone unit test project verifying deterministic seed-based dungeon plan generation and edge connectivity for the new "Echoes" mode.
  * Tests that rooms and paths satisfy topological requirements without deadlocks.
* **`RedungeonPC.InputContractTests`**:
  * Automated scenario tests verifying that input mappings, debounce rules, and state transitions comply with contract specifications across PC and gamepad modes.
  * *Build Issue*: Currently fails to compile due to referencing the self-contained `WinExe` executable (`NETSDK1151`).
* **Visual Regression Automation (`Knighter.QA/QaSession.cs`)**:
  * Injects automated inputs and captures backbuffer PNG screenshots at specific frame numbers (`REDUNGEON_QA_SCREEN`, `REDUNGEON_QA_CAPTURE_FRAME`), enabling automated screenshot diffing in CI/CD pipelines.

---

## 5. Architectural Flaws & Technical Debt Analysis

| Issue | Location | Impact | Severity |
|---|---|---|---|
| **Executable Reference in Tests (`NETSDK1151`)** | `Tests/RedungeonPC.InputContractTests.csproj` | Contract tests cannot build via `dotnet build` or CI | **Critical** |
| **Monolithic God Object (`Core.cs`)** | `Knighter/Core.cs` | High coupling; impossible to test gameplay in headless mode | **High** |
| **Android Asset Duplication (`CopyAssets`)** | `Android/MainActivity.cs` | Slow startup; doubles disk footprint on device | **High** |
| **Cross-Platform Source Wildcard Anti-Pattern** | `Android/Redungeon.Android.csproj` | Messy exclusions; risk of missing files or build divergence | **High** |
| **Code Duplication in Game Heads** | `Knighter/MobileGame.cs` vs `Android/MobileGame.cs` | Bugfixes in window/input logic are not shared | **Medium** |
| **Per-Frame Garbage Collection Pressure** | `MessageManager.SendAll`, `MouseTouchAdapter` | Allocates new lists every frame; causes micro-stutter on mobile | **Medium** |
| **Hardcoded 60-FPS Tick Loop** | `MobileGame.cs`, `Entity.cs` | Uncapped or 120Hz/144Hz displays cause simulation speedups | **Medium** |
| **Obsolete Networking API (`WebClient`)** | `LevelModules.cs`, `CrossPromotion.cs` | `SYSLIB0014` warning; blocks modern async networking | **Low** |

---

## 6. Proposed Modern Project Architecture

To resolve the architectural limitations, eliminate technical debt, and ensure maintainability, we recommend refactoring the solution into a **Clean Onion / Multi-Target Engine Architecture**.

### 6.1 Solution Structure Blueprint

```
Redungeon.sln
├── src/
│   ├── Redungeon.Core/                   [netstandard2.1 / net10.0 Class Library]
│   │   ├── Domain/                       (Entities, Tiles, Items, Spells, Stats)
│   │   ├── Gameplay/                     (LevelGenerator, Camera, Movement, Echoes)
│   │   ├── Graphics/                     (ResolutionManager, SpriteManager, Renderer)
│   │   ├── Input/                        (InputManager, Actions, Snapshots)
│   │   ├── Messages/                     (Zero-allocation MessageBus)
│   │   ├── States/                       (FSM State machine, UI widgets)
│   │   ├── Audio/                        (Sound & Music playback)
│   │   ├── Abstractions/                 (IHapticsService, IStorageService, IAssetProvider)
│   │   └── RedungeonGame.cs              (Platform-agnostic Game loop)
│   │
│   ├── Redungeon.DesktopGL/              [net10.0 WinExe (Desktop Platform Head)]
│   │   ├── Program.cs                    (Desktop entry point)
│   │   ├── Services/                     (DesktopStorageService, SdlRumbleService)
│   │   └── Content/                      (Desktop MGCB content references)
│   │
│   ├── Redungeon.Android/                [net10.0-android Exe (Mobile Platform Head)]
│   │   ├── MainActivity.cs               (Android lifecycle & View binding)
│   │   ├── Services/                     (AndroidVibratorService, AndroidStorageService)
│   │   └── Resources/                    (App icons, Android manifest)
│   │
│   └── Redungeon.Content/                [Shared Content Pipeline Project]
│       ├── Content.mgcb                  (Shaders, audio, fonts)
│       └── Assets/                       (Textures, JSON atlases, locales)
│
└── tests/
    ├── Redungeon.Core.Tests/             (xUnit: Unit tests for math, entities, serialization)
    ├── Redungeon.Echoes.Tests/           (xUnit: Procedural generation & graph validation)
    └── Redungeon.InputContract.Tests/    (xUnit: Headless input snapshot simulation)
```

### 6.2 Architectural Component Responsibilities

#### 1. `Redungeon.Core` (Shared Game Engine Library)
* **Target**: `net10.0` (Class Library referencing `MonoGame.Framework`).
* **Contents**: 100% of game logic, entities, procedural algorithms, UI widgets, and renderer logic.
* **Decoupled Platform Services**: Defines clean interfaces for platform-dependent features:
  ```csharp
  public interface IHapticsService
  {
      void Vibrate(string pulseId, float lowFreq, float highFreq, int ticks);
      void Stop();
  }

  public interface IStorageService
  {
      string Get(string key, string defaultValue = null);
      void Set(string key, string value);
      void Save();
  }

  public interface IAssetProvider
  {
      Stream OpenStream(string relativePath);
      string ReadAllText(string relativePath);
  }
  ```

#### 2. Platform Heads (`Redungeon.DesktopGL` & `Redungeon.Android`)
* Each platform head only contains:
  1. Operating-system entry point (`Program.cs` on PC; `MainActivity.cs` on Android).
  2. Concrete implementations of platform interfaces (`SdlHapticsService`, `AndroidHapticsService`, `TitleContainerAssetProvider`).
  3. Window initialization, application manifest, and icon resources.
* **Unified Game Class**: Both platforms instantiate `RedungeonGame : Game` from `Redungeon.Core`, passing their concrete platform services via Dependency Injection (IoC).

#### 3. Test Projects
* Because `Redungeon.Core` is a clean class library (not a `WinExe`), test projects can add a standard `<ProjectReference Include="..\src\Redungeon.Core\Redungeon.Core.csproj" />`.
* No duplicate entry point errors (`CS0017`), no executable reference errors (`NETSDK1151`), and no manual `<Compile Include="..." Link="..." />` workarounds.

---

## 7. Performance & Modernization Recommendations

### 7.1 Zero-Allocation Hot Path Optimization
* **Struct-Based Event Messaging**: Replace heap-allocated `List<MessageDesc>` and `Message` class instances with `readonly record struct` value types and pooled ring-buffers:
  ```csharp
  public readonly record struct GameEvent(EventType Type, int IntParam, object Payload);
  ```
* **Direct Touch Buffering**: Eliminate array allocations in `MouseTouchAdapter.GetState()` by reusing a fixed internal `TouchLocation[4]` buffer.
* **Object Pooling**: Expand the existing `ObjectPool<T>` in `Knighter.Helpers` to cover draw commands, particles, and level generation tiles.

### 7.2 Native Asset Streaming (`TitleContainer.OpenStream`)
* Replace `File.OpenRead` and `File.ReadAllText` with MonoGame's built-in `TitleContainer.OpenStream(path)`:
  ```csharp
  using Stream stream = TitleContainer.OpenStream(Path.Combine("Content", relativePath));
  return Texture2D.FromStream(graphicsDevice, stream);
  ```
* **Benefit**: `TitleContainer` works identically across Windows, Linux, and Android APK assets. This allows completely removing `MainActivity.CopyAssets()`, speeding up Android startup by several seconds and saving over 50MB of device storage.

### 7.3 Fixed Timestep Accumulator with Render Interpolation
* Rather than binding the game's simulation rate directly to the window display refresh rate, implement a standard fixed-update accumulator:
  ```csharp
  private double accumulator;
  private readonly double fixedDeltaTime = 1.0 / 60.0;

  protected override void Update(GameTime gameTime)
  {
      accumulator += gameTime.ElapsedGameTime.TotalSeconds;
      while (accumulator >= fixedDeltaTime)
      {
          SimulateTick(); // Pure 60-Hz gameplay tick
          accumulator -= fixedDeltaTime;
      }
  }

  protected override void Draw(GameTime gameTime)
  {
      float alpha = (float)(accumulator / fixedDeltaTime);
      Render(alpha); // Smooth sub-pixel interpolation for 120Hz/144Hz monitors
  }
  ```
* **Benefit**: Allows smooth rendering on high-refresh monitors and mobile screens without altering character movement speed or trap timings.

---

## 8. Implementation & Migration Roadmap

### Phase 1: Solution & Project Restructuring (Low Risk)
1. Create `Redungeon.sln` at repository root.
2. Create `src/Redungeon.Core` class library project.
3. Move `Knighter*` subdirectories into `src/Redungeon.Core`.
4. Update `RedungeonPC.csproj` to reference `Redungeon.Core.csproj`.
5. Update `Android/Redungeon.Android.csproj` to reference `Redungeon.Core.csproj`, removing the linked-file wildcard includes.
6. Fix `Tests/RedungeonPC.InputContractTests.csproj` by changing its reference from `RedungeonPC.csproj` to `Redungeon.Core.csproj`.

### Phase 2: Platform Abstraction Layer (Medium Risk)
1. Define `IHapticsService`, `IStorageService`, and `IAssetProvider` interfaces in `Redungeon.Core`.
2. Implement desktop services in `Redungeon.DesktopGL` (SDL2 haptics, AppData storage).
3. Implement mobile services in `Redungeon.Android` (Android Vibrator, internal files storage).
4. Migrate asset loading from `File.OpenRead` to `TitleContainer.OpenStream`, removing `MainActivity.CopyAssets()`.

### Phase 3: Garbage Collection & Hot Path Tuning (Medium Risk)
1. Refactor `MessageManager` to utilize pooled message queues.
2. Optimize `MouseTouchAdapter` to eliminate per-frame list allocations.
3. Replace obsolete `WebClient` instances with modern `HttpClient`.

### Phase 4: Modern Presentation & Interpolation (Feature Enhancement)
1. Implement the fixed-timestep accumulator with draw interpolation.
2. Add support for high-DPI scaling and dynamic refresh rates.
3. Enhance the 2D lighting engine with cached shadow geometry.

---
*End of Documentation — Generated automatically for Redungeon project repository.*

