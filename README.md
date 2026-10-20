# War Holler

Redneck comedy FPS vertical slice — barn main menu → character/weapon select → Free-for-All on **The Holler** with bots, early-1900s guns, and kill-streak voice lines.

Built as a **code-driven Unity 6 Built-in Render Pipeline** project: almost no manual scene setup. `GameBootstrap` constructs menus, the barn backdrop, The Holler map, characters, weapons, and bots at runtime from primitives.

## Requirements

- **Unity 6 LTS** (`6000.0.x` or newer 6000.x) — **Built-in Render Pipeline** (no URP / HDRP)
- Windows / macOS / Linux Editor
- This repo targets Editor **Play Mode** first. A Windows player build is done from the Editor (Unity batchmode was not available in the cloud agent that scaffolded this).

Project version file: `ProjectSettings/ProjectVersion.txt` → `6000.0.32f1`. Opening with any Unity 6 LTS is fine; Unity may upgrade package versions on first import.

## Open & play (Josh)

1. Install **Unity Hub** + **Unity 6 LTS** (3D / Built-in template is fine — do **not** need URP).
2. **Pull latest `main`** (or open a fresh clone).
3. **Quit Unity completely** if the project is open.
4. **Delete `Library/`** (and optionally `Temp/`) next to `Assets/` so stale URP imports cannot linger:
   ```bash
   # from the project root (the folder that contains Assets/, Packages/, ProjectSettings/)
   rm -rf Library Temp
   ```
5. **Add** / reopen this project folder in Hub. Let packages import (uGUI, test-framework, etc. — **no URP**).
6. Open scene `Assets/Scenes/Bootstrap.unity` (also listed in Build Settings).
7. Press **Play**. Console should log `[WarHoller] Built-in pipeline bootstrap` and `MaterialUtil using Built-in shader 'Standard'` (or Unlit/Color). LMB fire should bang + log `[WarHoller] FIRE …`.
8. Flow:
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

### Safe Mode (script compile / package failures)

If Unity opens in **Safe Mode**:

1. Confirm you are on **latest `main`** (`git pull origin main`).
2. **Quit Unity completely**, then delete stale generated folders next to `Assets/` (do not commit these — they are gitignored):
   ```bash
   # from the project root (the folder that contains Assets/, Packages/, ProjectSettings/)
   rm -rf Library Temp
   # optional: also clear old IDE project files so Unity regenerates them
   rm -f *.csproj *.sln
   ```
   Reopen the project in Unity Hub and let packages reimport.
3. Safe Mode dialog choices:
   - **Ignore** — open anyway (assets may be incomplete until scripts/packages load).
   - **Quit** — exit, wipe `Library/` + `Temp/`, pull/fix, reopen.
4. After scripts compile cleanly and packages resolve, Unity exits Safe Mode on its own.

**Packages note:** This project keeps `com.unity.test-framework` (1.4.5) and Rider IDE integration. `com.unity.ide.visualstudio` was removed — an older Visual Studio Editor package could fail to resolve `UnityEditor.TestRunner` / `nunit.framework` and leave `Unity.VisualStudio.Editor.dll` unloadable. Re-add **Visual Studio Editor 2.0.28+** from Package Manager only if you need VS project generation. **URP is not used** — do not add `com.unity.render-pipelines.universal`.

**When pasting Console errors for help**, separate them:

| Paste as | What to copy |
|----------|----------------|
| **Script compile errors** | Red lines with `error CS####` (e.g. `CS0119`, `CS0246`) — these are the ones that keep Safe Mode on. |
| **Package / assembly warnings** | Lines about `Library/ScriptAssemblies/*.dll will not be loaded`, `Unable to resolve reference`, Package Manager resolve failures — useful, but different from CS errors. |

Clear Console filters (Error / Warning / Log; uncollapse) before copying. Prefer **one paste of CS#### lines**, then optionally a second paste of package/assembly messages.

Known fixed issues already on `main`: UiFactory `Button`/`Slider` CS0119 (PR #2 / #3); pink screen + silent gunfire (PR #4 / #5); VS Editor package Safe Mode (PR #6). This project now uses **Built-in RP** (no URP asset assignment needed).

### Pink / magenta graphics

Project Settings → Graphics → Scriptable Render Pipeline Settings should be **None**. Quality levels should also have no custom RP asset. Runtime materials go through `WarHoller.Art.MaterialUtil.CreateColor` (`Standard` → `Unlit/Color` → `Sprites/Default` → `Legacy Shaders/Diffuse`), setting `_Color`. Cameras clear to a solid sky color (no skybox dependency).

**If still pink after pull:** quit Unity → delete `Library/` → reopen. No URP package or asset assignment is required.

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
    Art/         MaterialUtil (Built-in Standard / Unlit materials)
    Bootstrap/   GameBootstrap, RenderPipelineBootstrap (forces Built-in)
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
