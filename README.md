# War Holler

Redneck comedy FPS — barn main menu → character/weapon select → local matches with bots on rotating maps and modes. Early-1900s guns, Hatfields vs McCoys, kill-streak voice lines.

Built as a **code-driven Unity 6 Built-in Render Pipeline** project: almost no manual scene setup. `GameBootstrap` constructs menus, maps, characters, weapons, and bots at runtime from primitives.

**Phase 2** adds Capture the Flag (pig flag), Zone Control, WowMart, Meth Lab, lobby rotation, and an early-1900s art pass. Still **local + bots only** — no online netcode / Mirror.

## Requirements

- **Unity 6 LTS** (`6000.0.x` or newer 6000.x) — **Built-in Render Pipeline** (no URP / HDRP)
- Windows / macOS / Linux Editor
- This repo targets Editor **Play Mode** first. A Windows player build is done from the Editor.

Project version file: `ProjectSettings/ProjectVersion.txt` → `6000.0.32f1`. Opening with any Unity 6 LTS is fine; Unity may upgrade package versions on first import.

## Open & play (Josh)

1. Install **Unity Hub** + **Unity 6 LTS** (3D / Built-in template is fine — do **not** need URP).
2. **Pull the Phase 2 branch** (or `main` once merged):
   ```bash
   git fetch origin
   git checkout cursor/war-holler-phase-2   # or: git checkout main && git pull
   ```
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
   - **Main Menu** — full-bleed barn, **WAR HOLLER** title, settings (left), **PLAY** (bottom), **QUIT** (bottom-left). Shows **current lobby rotation** (map + mode). Click either mannequin → Character Select.
   - **PLAY** starts the shown rotation entry, then advances the playlist for next time. Bots fill empty slots (FFA ~12; team modes **6v6**).
   - **Character Select** — 3 Hatfields / 3 McCoys (one female each; Asmon Hatfield is a labeled parody). Early-1900s country look (hats, suspenders, dresses, boots). Pick lever-action / revolver / shotgun → **CONFIRM** (same rotation as menu).
   - **Match** — see modes below. Scoreboard hold **Tab** or **Esc**. End screen → Main Menu.

### How to try each mode / map

The menu label **Now playing:** is what the next **PLAY** / **CONFIRM** launches. Playlist cycles automatically. To force a specific combo without waiting:

1. From the project root (or any shell with PlayerPrefs access), you can also just mash **PLAY** / return to menu until the label matches — order is:

| # | Map | Mode |
|---|-----|------|
| 0 | The Holler | Free for All |
| 1 | The Holler | Pig Flag (CTF) |
| 2 | WowMart | Free for All |
| 3 | WowMart | Zone Control |
| 4 | Meth Lab | Free for All |
| 5 | Meth Lab | Pig Flag |
| 6 | The Holler | Zone Control |
| 7 | WowMart | Pig Flag |
| 8 | Meth Lab | Zone Control |

2. Or set PlayerPrefs key `WarHoller.LobbyRotationIndex` to `0`–`8` before Play (Editor: delete PlayerPrefs / use a tiny debug, or just cycle from the menu).

**Quick checks**

- **FFA** — first to **25 kills**, or **10:00** (most kills wins). Everyone is hostile.
- **Pig Flag (CTF)** — **6v6** (your faction + bots). Steal the enemy **pig**, carry it overhead, score at your base while your pig is home. First team to **3** captures (or most when time expires).
- **Zone Control** — **6v6**. Hold any of **3** pads (more friendlies than enemies inside). Points tick while held; first to **500** (or most at time). Contested = no points.

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

1. Confirm you are on the intended branch (`git pull`).
2. **Quit Unity completely**, then delete stale generated folders next to `Assets/`:
   ```bash
   rm -rf Library Temp
   rm -f *.csproj *.sln
   ```
   Reopen in Unity Hub and let packages reimport.
3. After scripts compile cleanly, Unity exits Safe Mode on its own.

**Packages note:** This project keeps `com.unity.test-framework` (1.4.5) and Rider IDE integration. `com.unity.ide.visualstudio` was removed — re-add **Visual Studio Editor 2.0.28+** only if you need VS project generation. **URP is not used** — do not add `com.unity.render-pipelines.universal`. **Do not add Mirror / Netcode** for Phase 2.

### Pink / magenta graphics

Project Settings → Graphics → Scriptable Render Pipeline Settings should be **None**. Runtime materials go through `WarHoller.Art.MaterialUtil.CreateColor` (`Standard` → `Unlit/Color` → `Sprites/Default` → `Legacy Shaders/Diffuse`). Cameras clear to a solid sky/interior color.

**If still pink after pull:** quit Unity → delete `Library/` → reopen. No URP package or asset assignment is required.

## What’s in Phase 2

- **Modes:** Free for All (25 kills), Capture the Flag / pig flag (first to 3), Zone Control (3 zones, first to 500). Shared **10:00** timer fallback.
- **Maps (procedural):** The Holler (detail upgrade), WowMart (store + parking lot), Meth Lab (underground rooms + corridors). Spawns, cover, CTF bases, zone markers.
- **Lobby rotation:** Play / Confirm consume `LobbyRotation` playlist; menu shows current entry.
- **Teams:** CTF / Zone Control fill **6v6** bots; friendly fire off.
- **Art:** Early-1900s Appalachian/country characters (hats, suspenders, long dresses, boots, earth tones). Asmon Hatfield keeps the labeled headset parody. Richer environment props via primitives.
- Guns, voice lines, Built-in RP `MaterialUtil`, Bootstrap flow — unchanged in spirit.

## Folder layout

```
Assets/WarHoller/
  Scripts/
    Art/         MaterialUtil (Built-in Standard / Unlit materials)
    Bootstrap/   GameBootstrap, RenderPipelineBootstrap (forces Built-in)
    Core/        ModeIds, MapIds, SettingsStore, LobbyRotation, PlayerLoadout
    Characters/  CharacterRoster, CharacterVisualBuilder
    Gameplay/    GameSession, CTF / Zone modes, PigFlag, ControlZone, PlayerController
    Weapons/     WeaponController, WeaponStats
    Bots/        BotDirector, BotController (FFA + objective AI)
    Maps/        MapLayout, MapPrimitives, TheHoller / WowMart / MethLab builders
    UI/          MainMenu, CharacterSelect, MatchHud, EndScreen
    Audio/       VoiceLinePlayer
```

## Build (Windows)

From the Editor: File → Build Settings → PC, Mac & Linux Standalone → Windows → Build.

No CI/player artifact is checked in; smoke batchmode was not run on the scaffold agent VM (Unity Editor not installed there).

## License / parody

Asmon Hatfield is an **inspired parody look**, not affiliated with any streamer or brand. Replace VO placeholders with original recordings before shipping.
