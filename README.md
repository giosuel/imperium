![Imperium Logo](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/imperium_full.png?raw=true)

# Imperium

Imperium is a powerful all-in-one debugging and admin tool to test and explore game mechanics and functionality in Lethal Company.

Imperium works on the client-side and server-side, meaning it works in singleplayer as well as multiplayer, as long as Imperium is installed on the host.

> [!IMPORTANT]
> Imperium was made with the intent to be a tool to debug game functionalities and provide more insight into the weird and wonderful mechanics of Lethal Company.
>
> I strongly stand against cheating and trolling in public lobbies. If you really want to cheat, at least put some effort into making your own hacked client!

![imperium-control-center](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/imperium-control-center.png?raw=true)

## Features

- Modular and user-centered UI system with floating and resizable windows.
- In-game overlays and indicators for hitboxes of shotguns, shovels, landmines, etc.
- Customizable 3-axis freecam with built-in teleportation, night vision and custom FOV settings.
- Built-in minimap and full-screen map with layer selection, zoom and more.
- Visualization of game object hitboxes, spawn locations, tile borders and much more.
- Eight different interface themes to pick from.
- Tape measure to measure in-game distances with custom layer selection support.
- Event log that logs in-game events related to entity spawning, targeting and more.
- Entity, scrap and map hazard; spawning, de-spawning and teleportation.
- Instant ship landing and takeoff and various other animation skips.
- Ability to kill and revive players including yourself.
- Entity spawn prediction powered by the Oracle Prediction Algorithm.
- Easy adjustment of game variables such as credits, quota deadline, spawn rates and many more.
- Player tweaks such as god mode, infinite sprint, infinite battery, night vision, invisibility, custom movement speed, custom jump height, and many more.
- Player teleportation via coordinate input or interactive location picking alongside a waypoint manager.
- Noise detection overlay that marks recently played noises with temporary markers.
- Option to toggle the employee HUD overlay.
- Enabling / Disabling of various render layers, post-processing effects, render passes and more.
- Built-in save file editor for save files (Level options, Furniture location, Player XP, etc.)
- Allows bypassing of the loading screen and instantly loading a save file on start-up.
- Fully compatible with [Unity Explorer](https://github.com/sinai-dev/UnityExplorer) (Including mouse look fix).

## Getting Started

Imperium can be downloaded directly from [Thunderstore](https://thunderstore.io/c/lethal-company/p/giosuel/Imperium/) or installed manually from the [releases page](https://github.com/giosuel/imperium/releases). If you are downloading Imperium through Thunderstore, I highly recommend to use the [Gale](https://thunderstore.io/c/lethal-company/p/Kesomannen/GaleModManager/) mod manager to manage your mods.

### Interface Navigation

Imperium can be navigated using functional keys, or F-Keys. All the default key bindings can be changed through [InputUtil](https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/)'s keybind interface in the Lethal Company settings.

Imperium is divided into several main interfaces which can be opened directly with functional keys (e.g. `F1` for the Imperium UI). Alternatively, its possible to switch between these interfaces with the dock on the left.

The main interface, called the "Imperium UI" is further divided into floating windows which contain Imperium's main settings and functionalities. These windows can be toggled on and off through the Imperium dock at the top of the screen. Altenatively, some of them can be opened through functional keys (e.g. `F3` to open the teleportation window).

| Default Keybind | Action                                                      |
| --------------- | ----------------------------------------------------------- |
| F1              | Opens the Imperium UI.                                      |
| F2              | Opens Imperium's spawn UI.                                  |
| F3              | Opens the teleportation window within the Imperium UI.      |
| F6              | Opens the Oracle spawn prediction UI.                       |
| F8              | Opens the full-screen map UI.                               |
| T               | Toggles interactive teleportation.                          |
| F               | Opens the Imperium freecam.                                 |
| M               | Toggles the Imperium minimap.                               |
| X               | Toggles the freecam's picture-in-picture mode.              |
| F11             | Toggles the freecam's picture-in-picture mode's fullscreen. |
| Z               | Toggles the vanilla employee HUD.                           |

## Imperium's Minimap / Map

Imperium comes with its own minimap and full-screen map. The map can be used to follow yourself or other employees, as well as track entities or even map hazards. By default, the map is set to top-down view but you can adjust the viewing angle in the full-screen map.

![imperium-map](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/imperium-map.png?raw=true)

![imperium-minimap](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/imperium-minimap.png?raw=true)

## Imperium's Visualizers

Imperium comes with a multitude of built-in visualizers for colliders, position trackers, map hazards and more. The visualizers are divided into 5 main categories.

### Overlays and Colliders

Overlays are simple position trackers which highlight certain important coordinates in the game such as spawn locations, AI nodes and more. Collider visualizers highlight box, sphere and capsule colliders of game objects.

### Insights

Insights provide screen-space overlays for any game objects in the scene.

![imperium-visualizers](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/imperium-visualizers.png?raw=true)

![imperium-insights](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/imperium-insights.png?raw=true)

![imperium-los-visualizer](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/imperium-los-vis.png?raw=true)

## Imperium's Freecam

Imperium comes with a built-in freecam that can be activated with **F**. The freecam camera is an exact copy of the player's gameplay camera with the exception of the culling layers that can be toggled in the layer selector. The layer selector can be toggled with **L** and is activated by default.

![freecam](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/freecam-layers-imperium.png?raw=true)

The freecam can be moved with **WASD** controls and **Q** and **E** for up and down movement.

| Keybind | Action |
| ----------------- | ----------- |
|         UpArrow           | Moves layer selection up in layer selector. |
| DownArrow | Moves layer selection down in layer selector. |
|        Space           | Toggles selected layer in layer selector. |
|        LeftArrow           | Increases freecam field of view. |
|        RightArrow            | Descreases freecam field of view. |
|         ScrollUp          | Increases freecam movement speed. |
|        ScrollDown           | Decreases freecam movement speed. |
|        R           | Resets the freecam's position, FOV and movement speed. |
|        T           | Teleports the player model to the freecam. |
| L | Toggles layer selector. |

## The Spawning Console

Tha spawning console allows you to spawn entities, scrap and map hazards into the scene.

**Command Syntax:** `objectName [amount] [health / value]`

![spawning-ui](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/spawning-ui.png?raw=true)

## The Tape Measure

The tape measure is a virtual tool to measure distances in the game. The tape measure supports axis snapping when holding down the `Alt` key. It also tries to snap to surfaces as well as possible.

![tape-measure](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/tape-measure.png?raw=true)

## Ship and Moon Control

![ship-moon-control](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/ship-moon-control.png?raw=true)

## The Event Log

The event log allows you to track specific game events such as entity spawns, noise detection, entity targeting and LoS detection and more.

![event-log](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/event-log.png?raw=true)

## Moon / Challenge Moon Information

![moon-information](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/moon-info.png?raw=true)

## The Save File Editor

Imperium's built-in save file editor can edit any non-vector data fields from the general save file (`LCGeneralSaveData`) and the individual game saves (`LCSaveFileX`).

![save-file-editor](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/save-file-editor.png?raw=true)

> [!CAUTION]
> It is not recommended to use the save file editor unless you know what you are doing.
>
> To provide maximum control over the save files, Imperium doesn't implement any input validation, meaning it is very easy to screw up the saves and render the game **completely unplayable**!
>
> Always make sure to have a backup before changing anything!

## Imperium Settings

The Imperium settings interface provides access to various preferences of Imperium.

<img src="https://github.com/giosuel/imperium/blob/main/assets/imperium-settings.png?raw=true" alt="imperium-settings" style="zoom: 67%;" />

## Oracle's Spawn Prediction

Oracle is an algorithm developed by the Imperium team to predict indoor, outdoor and daytime entity spawns over the course of the day.

Oracle predicts daytime, outdoor and indoor entities including their spawn times and spawn positions. Clicking the position text in an oracle entry teleports you to the spawn point.

Entities spawned with Imperium do not count towards current level power or their max count, meaning, they **do not influence** the entity spawning.

> [!NOTE]
> Oracle is currently in beta stage and is therefore not guaranteed to always work properly.

![imperium-oracle](https://raw.githubusercontent.com/giosuel/imperium/refs/heads/main/assets/imperium-oracle.png?raw=true)

## Bug Reports and Feature Requests

Feel free to submit bug reports or feature requests asissues on Imperium's [Github](https://github.com/giosuel/imperium) page. Please include your BepInEx log file or an excerpt to make my life easier.

## Credits

Imperium was designed and implemented by the Imperium team ([giosuel](https://github.com/giosuel)).

Various icons were provided by [FlatIcon](https://www.flaticon.com/).

### Special Thanks To

- [ratijas](https://github.com/ratijas) for helping me with bug hunting and issue management!
- [sinai-dev](https://github.com/sinai-dev) for creating [Unity Explorer](https://github.com/sinai-dev/UnityExplorer), my favorite mod of all time!
- [Adi](https://thunderstore.io/c/lethal-company/p/AdiBTW/) for helping with the [LOS](https://github.com/AdalynBlack/LC-EnemyDebug) visualizers!
- [digger1213](https://github.com/digger1213) for writing the shotgun visualizer!
- [Xilo](https://github.com/Xilophor) for helping me with the networking and writing the [LethalNetworkingAPI](https://github.com/Xilophor/LethalNetworkAPI)!
- [CTN](https://github.com/CTNOriginals) and [Flero](https://github.com/flerouwu) for providing me with code for the Quickload logic!
- [Nebby](https://thunderstore.io/c/lethal-company/p/Nebulaetrix/) for helping with the documentation and the testing!
- [Swaggies](https://thunderstore.io/c/lethal-company/p/Swaggies), [aminoob](https://thunderstore.io/c/lethal-company/p/aminoob/) and TulipNova for the help with the testing of Imperium!
- [Dancemoon](https://thunderstore.io/c/lethal-company/p/dancemoon/) who made [DanceTools](https://thunderstore.io/c/lethal-company/p/dancemoon/DanceTools/), which was the initial inspiration for Imperium!
- [chaser324](https://github.com/Chaser324) for providing the wireframe shaders for the collider visualizations!
- [Evaisa](https://thunderstore.io/c/lethal-company/p/Evaisa/) for writing the [Unity Netcode Patcher](https://github.com/EvaisaDev/UnityNetcodePatcher) and [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/)! 