# Luffy Abilities – Blade & Sorcery Mod

Become the rubber man. This Blade & Sorcery mod adds **Monkey D. Luffy**'s Gum-Gum powers:

| Ability | What it does |
|---|---|
| **Gum-Gum Fling** (Pistol) | Punch fast with an empty fist: your arm stretches up to 8 m, hits enemies, items or walls, and snaps back. |
| **Gum-Gum Rocket** | Keep the fist closed when the stretched fist hits a wall: it pulls you there. |
| **Gear Second** | You move 2.5× faster and jump higher, stretched punches fly 2.5× faster and recover quicker, and steam comes off your arms. |
| **Gear Third** | Both fists inflate to giant size and hit **3× harder**, with close-range punches and stretched punches. |
| **Gear 2 + Gear 3** | Both at once: fast giant flings. |

Supported game versions: **Blade & Sorcery U12** and **Blade & Sorcery 1.0 and newer** (PCVR).
You don't need to compile anything. Download the zip for your game version and drop it in.

---

## Installation

### 1. Download the right zip

Download it from the [`dist/`](dist) folder of this repository (or from the Releases page, if there is one):

| Your game version | Download |
|---|---|
| **1.0 or newer** (current Steam / Oculus PC version) | [`LuffyAbilities-1.0.0-BaS-1.0.zip`](dist/LuffyAbilities-1.0.0-BaS-1.0.zip) |
| **U12** (e.g. the Steam *legacy* beta branch) | [`LuffyAbilities-1.0.0-BaS-U12.zip`](dist/LuffyAbilities-1.0.0-BaS-U12.zip) |

> On a GitHub file page, click **Download raw file** (the ⬇ icon).
> The game version appears in the bottom corner of the main menu.

### 2. Extract it into the Mods folder

Open your game's mod folder:

| Store | Path |
|---|---|
| Steam | `C:\Program Files (x86)\Steam\steamapps\common\Blade & Sorcery\BladeAndSorcery_Data\StreamingAssets\Mods\` |
| Oculus/Meta PC | `C:\Program Files\Oculus\Software\Software\warpfrog-blade-sorcery\BladeAndSorcery_Data\StreamingAssets\Mods\` |

> Tip: in Steam, right-click **Blade & Sorcery → Manage → Browse local files**.
> If the `Mods` folder doesn't exist yet, create it.

Extract the zip **into** `Mods`. You should end up with:

```
StreamingAssets/
└── Mods/
    └── LuffyAbilities/
        ├── LuffyAbilities.dll
        └── manifest.json
```

⚠️ Avoid `Mods/LuffyAbilities/LuffyAbilities/...` (a folder inside a folder). If you see that, move the inner folder up one level.

### 3. Start the game

Open the in-game menu: **Mods** should list *Luffy Abilities* without errors.
Its settings are under **Mods → Luffy Abilities** (see [Settings](#settings)).

### Uninstall

Delete the `Mods/LuffyAbilities` folder.

---

## How to use it

All controls use your normal VR controllers. **Grip** = make a fist. **Trigger** = use button.

### Gum-Gum Fling (stretch punch)
1. Keep the hand **empty** and hold **grip** (fist).
2. **Punch forward fast** (at least 3.5 m/s by default, about a real, committed punch).
3. The arm stretches out, hits the first thing in its path, and snaps back.
   - **Enemies** get knocked back and take blunt damage; strong hits knock them off balance.
   - **Items / physics objects** get launched.
   - **Walls / ground**: if you are **still holding grip** when it hits, you get pulled there (**Gum-Gum Rocket**).
4. Slow your hand down and punch again for the next fling. Both hands work on their own.

### Gear Second
1. **Crouch**, either for real or with the crouch button.
2. Hold an **empty fist** (grip) down **near your knee** and keep it still for **1 second**.
   You'll feel the vibration build up.
3. Strong buzz = **Gear 2 ON**: you're much faster, steam rises from your arms, and stretched punches are way faster.
4. Do the same pose again to turn it **OFF**.

### Gear Third
1. Hold an **empty hand against your mouth** (as if blowing into your thumb) for a moment.
2. An **orange ring** appears in front of your mouth.
3. Press the **trigger of that same hand**: both fists **inflate**. Stretched punches and normal close-range punches hit much harder.
4. To deflate, do it again (the ring is **blue** while Gear 3 is active).

### Combining
Use Gear 2 and Gear 3 at the same time for fast giant stretch punches.

### Limits (Mana / Focus)
- **Infinite Mana/Focus** (e.g. sandbox with infinite enabled): no time limit.
- **Limited Mana/Focus**: each gear uses up your bar while it is active. In U12 that is **Mana**, in 1.0+ it is **Focus**:
  - Gear 2 alone: **60 s** max (from a full bar)
  - Gear 3 alone: **60 s** max
  - Gear 2 + Gear 3: **30 s** max
  - Normal regeneration is paused while a gear is active, so these are true maximums.
  - When the bar is empty, the gears switch off.
- **Fling** is always available. It costs 0.00000000000000000000001 % of your bar per use, which is effectively free.
- **No aftermath:** after turning a gear off, you're not slowed, shrunk or weakened.

---

## Settings

In game: **Menu → Mods → Luffy Abilities**.

| Category | Setting | Default |
|---|---|---|
| General | Gear duration (at a full bar, per gear) | 60 s |
| General | Haptics | On |
| Gum-Gum Fling | Enable / punch speed needed / arm reach / impact force / Gum-Gum Rocket | On / 3.5 m/s / 8 m / 1× / On |
| Gear Second | Enable / movement speed / fling speed / pose hold time / steam | On / 2.5× / 2.5× / 1 s / On |
| Gear Third | Enable / fist size / impact | On / 2.5× / 3× |

---

## Troubleshooting

| Problem | Fix |
|---|---|
| Mod menu says **"Incompatible with game version"** | You picked the wrong zip, so use the other one. On an unusual U12 build, open `manifest.json` and set `"GameVersion"` to the version from the error message (e.g. `"0.12.3.0"`). |
| Mod isn't listed at all | Check the folder layout (see step 2). The `manifest.json` must be directly inside `Mods/LuffyAbilities/`. |
| Arm doesn't stretch | Your hand must be **empty**, **grip** must be held, and you need to punch **forward** (away from your face). Try lowering *Punch speed to fling* in the settings. |
| Flings happen by accident during normal fights | Raise *Punch speed to fling*, or turn the fling off. |
| Gear 2 won't trigger | Crouch lower, keep the fist **still** near your knee, and check that the hand is empty. |
| Gear 3 ring doesn't appear | The hand must be **empty** and right at your mouth. |
| Something else | Check the log at `%USERPROFILE%\AppData\LocalLow\Warpfrog\BladeAndSorcery\Player.log` and search for `LuffyAbilities`. |

**Platform note:** this is a PCVR code mod. Nomad (standalone Quest) has not been tested.

---

## For developers

### Building it yourself (optional)

Requirements: [.NET SDK 8+](https://dotnet.microsoft.com/download) and a copy of the game.
The game's assemblies are **not** in this repository (they're copyrighted). You point the build at your own install:

```bash
# Windows (PowerShell / cmd): one game version
dotnet build src/LuffyAbilities -c Release -p:GameVersion=V1 ^
  -p:BaSManaged="C:\Program Files (x86)\Steam\steamapps\common\Blade & Sorcery\BladeAndSorcery_Data\Managed"

# Both versions + zips (Linux / macOS / Git Bash / WSL)
build/build.sh --v1 "<1.0 install>/BladeAndSorcery_Data/Managed" --u12 "<U12 install>/BladeAndSorcery_Data/Managed"
```

`GameVersion` is `V1` (1.0+, the default) or `U12`. Each zip gets a DLL compiled against its own game version, so there are no runtime API mismatches.

### Checking compatibility after a game update

`tools/RefCheck` reports every type or method the mod uses that a game build no longer has, without launching the game:

```bash
dotnet run --project tools/RefCheck -- src/LuffyAbilities/bin/Release/V1/LuffyAbilities.dll "<game>/BladeAndSorcery_Data/Managed"
```

### Project layout

```
src/LuffyAbilities/
├── LuffyScript.cs          ThunderScript entry point, game loop and lifecycle
├── Options.cs              in-game ModOptions
├── Compat/GameCompat.cs    the ONLY place with U12 vs 1.0 differences (Mana ↔ Focus, locomotion body)
├── Core/                   PlayerContext / HandContext (tracking), HoldTimer, GearState, GearEnergy (drain)
├── Abilities/              GumGumFling, GiantFistPunch, GearSecond, GearThird, Impact (shared hit logic)
└── Visuals/                VisualFactory, RubberArm, MouthRing, HandScaler (all built from code, no asset bundles)
mod/manifest.template.json  manifest; build.sh fills in versions
build/build.sh              builds every version and packs dist/*.zip
tools/RefCheck/             API compatibility checker
```

How it works, briefly:
- Gestures are measured relative to your play space (hand speed in rig space, crouch from an auto-calibrated standing height), so walking, Gear 2 speed and snap-turning never count as punches.
- Visuals are generated at runtime (line renderer arm, procedural fist, steam particles, scaled hand bones), so the mod has no asset bundle that could break between game versions.
- Gear 3 only scales the *visual* hand bones. Physics colliders are untouched, and the extra power is applied by the mod's own impact logic.

---

## Credits

- Mod: Cametendo
- *One Piece* and Monkey D. Luffy © Eiichiro Oda / Shueisha / Toei Animation. This is an unofficial fan project.
- *Blade & Sorcery* © WarpFrog.

License: see [LICENSE](LICENSE).
