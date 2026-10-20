# War Holler

Redneck comedy FPS vertical slice — barn main menu → character/weapon select → Free-for-All on **The Holler** with bots, early-1900s guns, and kill-streak voice lines.

Built as a **code-driven Unity 6 URP** project: almost no manual scene setup. `GameBootstrap` constructs menus, the barn backdrop, The Holler map, characters, weapons, and bots at runtime from primitives.

## Requirements

- **Unity 6 LTS** (`6000.0.x` or newer 6000.x) with **Universal Render Pipeline (URP)**
- Windows / macOS / Linux Editor
- This repo targets Editor **Play Mode** first. A Windows player build is done from the Editor (Unity batchmode was not available in the cloud agent that scaffolded this).

Project version file: `ProjectSettings/ProjectVersion.txt` → `6000.0.32f1`. Opening with any Unity 6 LTS is fine; Unity may upgrade package versions on first import.

## Open & play (Josh)

1. Install **Unity Hub** + **Unity 6 LTS** (URP template modules optional — packages are in `Packages/manifest.json`).
2. **Pull latest `main`** (or open a fresh clone). PR #1 (slice) and PR #2 (UiFactory CS0119) are already merged — an older checkout will still enter Safe Mode.
3. **Add** this project folder in Hub → Open.
4. On first open, let Unity import packages (URP, uGUI, Input System package is listed but **this slice uses Legacy Input** via `Input.*` — see below).
5. Open scene `Assets/Scenes/Bootstrap.unity` (also listed in Build Settings).
6. Press **Play**.
7. Flow:
   - **Main Menu** — full-bleed barn, **WAR HOLLER** title, settings (left), **PLAY** (bottom), **QUIT** (bottom-left). Click either mannequin → Character Select.
   - **PLAY** quick-starts FFA on The Holler with current/default loadout and fills empty slots with bots (~12 participants).
   - **Character Select** — 3 Hatfields / 3 McCoys (one female each; Asmon Hatfield is a labeled parody). Pick lever-action / revolver / shotgun → **CONFIRM**.
   - **Match** — first to **25 kills** or **10:00** (most kills wins). Scoreboard hold **Tab** or **Esc**. End screen → Main Menu.

### Controls

| Action | Binding |
|--------|---------|
| Move | WASD |
| Look | Mouse |
| Jump | Space |
| Shoot | LMB |
| ADS zoom | RMB |
| Reload | R |
| Sprint | Left Shift |
| Scoreboard | Hold Tab or Esc |

**Input note:** Legacy Input Manager (`Input.GetAxis` / mouse buttons). The New Input System package is in the manifest for a future migration; `activeInputHandler` is set to **Both** so Play Mode works without an Input Actions asset.

### Safe Mode (script compile failures)

If Unity opens in **Safe Mode**:

1. Confirm you are on **latest `main`** (`git pull origin main`). Do not keep working from a zip/tarball taken before PR #2.
2. Console → clear all filters (Error / Warning / Log / collapsed) → copy **all red errors** and send them if Safe Mode persists after pull.
3. Safe Mode dialog choices:
   - **Ignore** — open the project anyway (assets may be incomplete until scripts compile).
   - **Quit** — exit Unity, pull/fix, reopen the project.
4. After scripts compile cleanly, Unity exits Safe Mode automatically on the next open/reimport.

The known CS0119 (`UiFactory` methods named `Button`/`Slider` shadowing `UnityEngine.UI.*`) was fixed on `main` (PR #2) and further hardened by renaming those factories to `CreateButton` / `CreateSlider`.

### URP note

`Packages/manifest.json` includes `com.unity.render-pipelines.universal`. On first open, if materials look pink:

1. Window → Rendering → Render Pipeline Converter (or create a URP Asset via Create → Rendering → URP Asset).
2. Assign the URP asset under Project Settings → Graphics / Quality.
3. Shaders already prefer `Universal Render Pipeline/Lit` and fall back to `Standard`.

## What’s in the slice

- Main menu barn atmosphere (procedural wood, hay, light shafts)
- Brand hero title **War Holler**
- Video settings (resolution, quality, fullscreen) + audio (master, SFX, voice) via `SettingsStore` / PlayerPrefs
- Character roster: Hatfields vs McCoys, parody Asmon Hatfield (headset + long hair)
- Weapons: lever-action rifle, revolver, shotgun (hip-fire + ADS, distinct stats / pellets)
- Map **The Holler**: valley, stream, shacks, rusted cars, clutter, spawns, cover
- Bots fill slots; chase, shoot, wander cover
- Voice events (procedural tone clips + exact phrase logs): kill / triple / five-streak + combat barks
- Kill feed, timer, scoreboard, respawn, end screen

## Scaffold only (extension points)

Commented stubs / enums — not playable yet:

- Modes: `CaptureTheFlag` (pig flag), `ZoneControl` — see `ModeIds`, `GameSession`, `LobbyRotation`
- Maps: `WowMart`, `MethLab` — see `MapIds`, `MapBuilderStubs`, `LobbyRotation`
- Online quick-join playlist: `LobbyRotation.QuickJoinPlaylist`

## Folder layout

```
Assets/WarHoller/
  Art/ Audio/ Characters/ Gameplay/ Maps/ UI/
  Scripts/
    Bootstrap/   GameBootstrap
    Core/        ModeIds, MapIds, SettingsStore, LobbyRotation, PlayerLoadout
    Characters/  CharacterRoster, CharacterVisualBuilder
    Gameplay/    GameSession, PlayerController, Health, CombatantMotor
    Weapons/     WeaponController, WeaponStats
    Bots/        BotDirector, BotController
    Maps/        TheHollerBuilder, BarnBackdropBuilder, stubs
    UI/          MainMenu, CharacterSelect, MatchHud, EndScreen
    Audio/       VoiceLinePlayer
```

## Build (Windows)

From the Editor: File → Build Settings → PC, Mac & Linux Standalone → Windows → Build.

No CI/player artifact is checked in; smoke batchmode was not run on the scaffold agent VM (Unity Editor not installed there).

## License / parody

Asmon Hatfield is an **inspired parody look**, not affiliated with any streamer or brand. Replace VO placeholders with original recordings before shipping.
