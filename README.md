![Imperium Logo](https://github.com/giosuel/imperium/blob/main/assets/imperium_full_beta_alpha.png?raw=true)

# Imperium

Imperium is a powerful and highly performant all-in-one debugging tool to test and explore game mechanics and functionality in Lethal Company.

It is a client-side and server-side mod, meaning it works in singleplayer and multiplayer, as long as Imperium is also installed on the host.

> [!IMPORTANT]
> Imperium was made with the intent to be a tool to test game functionality and provide more insight into the weird and wonderful mechanics of Lethal Company.
>
> The Imperium team strongly stands against cheating and trolling in public lobbies. If you really want to cheat, at least put some effort into making your own hacked client!

![imperium-control-center](https://github.com/giosuel/imperium/blob/main/assets/imperium-control-center.png?raw=true)

## Features

- Modular and user-centered UI system.
- In-game overlays and indicators for hitboxes of shotguns, shovels, landmines, etc.
- Customizable 3-axis freecam with built-in teleportation, night vision and custom FOV settings.
- Built-in minimap and full-screen map with layer selection, zoom and more.
- Visualization of game object hitboxes, spawn locations, tile borders and much more.
- Eight different interface themes to pick from.
- Entity, scrap and map hazard; spawning, de-spawning and teleportation.
- Instant ship landing and takeoff and various other animation skips.
- Ability to kill and revive players including the yourself.
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
- Fully compatible with Version 50 of Lethal Company.

## Imperium's Minimap / Map
Imperium comes with it's own built-in map and minimap.

![imperium-map](https://github.com/giosuel/imperium/blob/main/assets/imperium-map.png?raw=true)

![imperium-minimap](https://github.com/giosuel/imperium/blob/main/assets/imperium-minimap.png?raw=true)

## Imperium's Visualizers

The visualizers windows allows access to all the static and dynamic collider visualizers, gizmos and screen-space overlays.

![imperium-visualizers](https://github.com/giosuel/imperium/blob/main/assets/imperium-visualizers.png?raw=true)

![imperium-insights](https://github.com/giosuel/imperium/blob/main/assets/imperium-insights.png?raw=true)

![imperium-los-visualizer](https://github.com/giosuel/imperium/blob/main/assets/imperium-los-vis.png?raw=true)

## Imperium's Freecam

Imperium comes with a built-in freecam that can be activated with **F**. The freecam camera is an exact copy of the player's gameplay camera with the exception of the culling layers that can be toggled in the layer selector. The layer selector can be toggled with **L** and is activated by default.

![freecam](https://github.com/giosuel/imperium/blob/main/assets/freecam-layers-imperium.png?raw=true)

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

![spawning-ui](https://github.com/giosuel/imperium/blob/main/assets/spawning-ui.png?raw=true)

## The Save File Editor

Imperium's built-in save file editor can edit any non-vector data fields from the general save file (`LCGeneralSaveData`) and the individual game saves (`LCSaveFileX`).

![save-file-editor](https://github.com/giosuel/imperium/blob/main/assets/save-file-editor.png?raw=true)

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

![imperium-oracle](https://github.com/giosuel/imperium/blob/main/assets/imperium-oracle.png?raw=true)

## Bug Reports and Feature Requests

Feel free to submit bug reports or feature requests asissues on Imperium's [Github](https://github.com/giosuel/imperium) page. Please include your BepInEx log file or an excerpt to make our life easier.

## Credits

Imperium was designed and implemented by the Imperium team ([giosuel](https://github.com/giosuel)).

Various icons were provided by [FlatIcon](https://www.flaticon.com/).

### Special Thanks To

- [Nebby](https://thunderstore.io/c/lethal-company/p/Nebulaetrix/) for helping with the documentation and the testing!

- [Adi](https://thunderstore.io/c/lethal-company/p/AdiBTW/) for her amazing [LOS](https://github.com/AdalynBlack/LC-EnemyDebug) visualizers!

- [Swaggies](https://thunderstore.io/c/lethal-company/p/Swaggies), [aminoob](https://thunderstore.io/c/lethal-company/p/aminoob/) and star0138 for the help with the testing of Imperium!

- [sinai-dev](https://github.com/sinai-dev) for creating [Unity Explorer](https://github.com/sinai-dev/UnityExplorer), my favorite mod of all time!

- [Dancemoon](https://thunderstore.io/c/lethal-company/p/dancemoon/) who made [DanceTools](https://thunderstore.io/c/lethal-company/p/dancemoon/DanceTools/), which was the initial inspiration for Imperium!

- [chaser324](https://github.com/Chaser324) for providing the wireframe shader for the collider visualizations!

- [Sligili](https://thunderstore.io/c/lethal-company/p/Sligili/) for writing HDLethalCompany!

- [Evaisa](https://thunderstore.io/c/lethal-company/p/Evaisa/) for wA the [unity netcode patcher](https://github.com/EvaisaDev/UnityNetcodePatcher)!