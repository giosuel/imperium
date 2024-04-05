![Imperium Logo](assets/imperium_full_beta_alpha.png)

# Imperium (Beta)

Imperium is a powerful and highly performant all-in-one debugging tool to test and explore game mechanics and functionality in Lethal Company.

It is a client-side and server-side mod, meaning it works in singleplayer and multiplayer, as long as Imperium is also installed on the host.

> [!CAUTION]
>
> Imperium was made with the intent to be a tool to test game functionality and provide more insight into the weird and wonderful mechanics of Lethal Company.
> 
>The Imperium team strongly stands against cheating and trolling in public lobbies. If you really want to cheat, at least put some effort into making your own hacked client!

## Features

- Modular and user-centered UI system.
- In-game overlays and indicators for hitboxes of shotguns, shovels, landmines, etc.
- Customizable 3-axis freecam with built-in teleportation and custom FOV settings.
- Visualization of game object hitboxes, spawn locations, tile borders and much more.
- Entity, scrap and map hazard; spawning, de-spawning and teleportation.
- Ability to kill and revive players including the yourself.
- Entity spawn prediction powered by the Oracle Prediction Algorithm.
- Easy adjustment of game variables such as credits, quota deadline, spawn rates and many more.
- Player tweaks such as god mode, infinite sprint, night vision, invisibility and many more.
- Player teleportation via coordinate input or interactive location picking alongside a waypoint manager.
- Enabling / Disabling of various render layers, post-processing effects, render passes and more.
- Built-in save file editor for save files (Level options, Furniture location, Player XP, etc.)
- Allows bypassing of the loading screen and instantly loading a save file on start-up.
- Fully compatible with [Unity Explorer](https://github.com/sinai-dev/UnityExplorer) (Including mouse look fix).
- Fully compatible with the public beta v50 of Lethal Company.

## UI System and Keybinds

Imperium is divided into 5 separate main interfaces that can be opened separately with the functional keys **F1** - **F6**.

One UI can be opened at a time and opening another UI will result in the others being closed. This also works with the *Unity Explorer*. Many of the UIs consist of separate windows that can be collapsed if needed. It is currently not possible to re-bind any Imperium keybinds.

| Keybind | Action                                                      |
|---------|-------------------------------------------------------------|
| F1      | Opens the Imperium control center and the object explorer.  |
| F2      | Opens the object spawning menu.                             |
| F3      | Opens the teleportation interface and the waypoint manager. |
| F4      | Opens the weather forecaster.                               |
| F5      | Opens the Oracle spawn prediction.                          |
| F6      | Opens the ship navigator.                                   |
| T       | Opens the interactive teleportation indicator.              |
| F       | Toggles the Imperium freecam.                               |

## Imperium Control Panel

The Main Control Panel allows you to change game and player settings as well as enabling and disabling the various overlays and gizmos Imperium offers. The control panel also allows access the Moon Settings, Render Settings, Imperium Settings, Save File Editor and the Object Settings menu.

![imperium-control-center](https://github.com/giosuel/imperium/blob/main/assets/imperium-ui.png?raw=true)

- **Casting Indicators:** Raycast and physics collision indicators (*client-side*).
- **Gizmos:** In-game objects that visualize spawn indicators and timers (*client-side*).
- **Colliders:** Collider visualization of predefined game objects by tag or layer (*client-side*).
- **Overlays:** Various in-game overlays for path-finding, spawn locations, etc. (*client-side*).
- **Player Settings:** Player tweaks and adjustments (*client-side*).
- **Game Settings:** General game settings and adjustments (*client-side*).
- **Time Settings:** Time-related settings (*client- and server-side*).

## Object Explorer

The object explorer allows you to easily manage the various game objects currently loaded in a scene.

- Items and Scrap (Despawn, Drop, Teleport To)
- Entities (Despawn, Respawn, Teleport To)
- Players (Kill, Revive, Teleport To, Teleport Player)
- Map Hazards (Despawn, Teleport To)

## Moon Settings

The moon settings interface lets you view and change settings and variables of the current moon. Challenge moon information is always provided, even on non-challenge save files.

> [!TIP]
> All the moon settings, including the min spawns, max power, weather variables and spawn lists are not persistent and reset every time a new game is loaded.

![moon-settings](https://github.com/giosuel/imperium/blob/main/assets/moon-settings.png?raw=true)

## Render Settings

The render settings interface allows you to activate / deactivate render layers and objects that make up the scene. Many of the settings are only used when on a moon. It is also possible to change the resolution of the game in real-time.

<img src="https://github.com/giosuel/imperium/blob/main/assets/render-settings.png?raw=true" alt="render-settings" style="zoom:50%;" />

- **Lighting:** Contains light and sub objects.
- **Volumetrics:** Contains global and local volumes.
- **Frame Settings:** HDRP-specific camera settings for all the cameras in the scene except the map.
- **Post Processing:** Screen-space post-processing camera layers.
- **Player Overlays / Filters:** Contains various employee overlays and filters.

> [!Note]
>
> Due to the nature of buffering and rendering of unity cameras, a new render texture has to be generated every time the resolution is changed. The slider has a debounce timer to make sure the game doesn't crash but changing the resolutionstill lags the game for a few seconds.

## Save File Editor

Imperium's built-in save file editor can edit any non-vector data fields from the general save
file (`LCGeneralSaveData`) and the individual game saves (`LCSaveFileX`).

![save-file-editor](https://github.com/giosuel/imperium/blob/main/assets/save-file-editor.png?raw=true)

> [!CAUTION]
> It is not recommended to use the save file editor unless you know what you are doing.
>
> To provide maximum control over the save files, Imperium doesn't implement any input validation, meaning it is very
> easy to screw up the saves and render the game **completely unplayable**!
>
> Always make sure to have a backup before changing anything!

## Imperium Freecam

Imperium comes with a built-in freecam that can be activated with **F**. The freecam camera is an exact copy of the player's gameplay camera with the exception of the culling layers that can be toggled in the layer selector. The layer selector can be toggled with **L** and is activated by default.

![freecam](https://github.com/giosuel/imperium/blob/main/assets/freecam.png?raw=true)

The freecam can be moved with **WASD** controls and **Q** and **E** for up and down movement.

| Keybind    | Action                                                 |
|------------|--------------------------------------------------------|
| UpArrow    | Moves layer selection up in layer selector.            |
| DownArrow  | Moves layer selection down in layer selector.          |
| Space      | Toggles selected layer in layer selector.              |
| LeftArrow  | Increases freecam field of view.                       |
| RightArrow | Descreases freecam field of view.                      |
| ScrollUp   | Increases freecam movement speed.                      |
| ScrollDown | Decreases freecam movement speed.                      |
| R          | Resets the freecam's position, FOV and movement speed. |
| T          | Teleports the player model to the freecam.             |
| L          | Toggles layer selector.                                |

## Spawning Console

Tha spawning console allows you to spawn entities, scrap and map hazards into the scene.

There are three different spawn categories; **Entites**, **Items** and **Map Hazards**. The spawn category can be
changed with **Tab**. To spawn the first entity in the autocomplete list, press **Return**.

Submitting an empty input will result in the last comment being executed again.

| Category   | Command Syntax                 |
|------------|--------------------------------|
| Entity     | `entityName [amount] [health]` |
| Item       | `itemName [amount] [value]`    |
| Map Hazard | `mapHazardName [amount]`       |

![spawning-ui](https://github.com/giosuel/imperium/blob/main/assets/spawning-ui.png?raw=true)

## Imperium Settings

The Imperium settings interface provides access to various preferences of Imperium.

### Logging

Imperium uses message boxes to provide more user-friendly logs; these message boxes can be turned off by unchecking *General Logging*.

Oracle uses these message boxes to log spawn predictions, meaning, it generates a message box every time the prediction is updated.

It is recommended to keep *Oracle logging* turned off when not needed, for performance reasons.

### Quickload

When enabled, Quickload will skip the start-up screen and / or the game menu.

If the menu is skipped, the number of the save file that will be loaded can be provided in the input box. If *Clean File* is selected, the save file will be deleted every time a game is loaded.

If *Reload On Quit* is selected, the game will instantly be reloaded after you exit a game via the quickmenu (Esc).

### Other Settings

- **Optimize Logs:** Adds harmony patches to verbose LC functions to reduce logs and increase the overall game performance and log readability.
- **UE Mouse Fix:** If turned on, the *Unity Explorer* UI will capture the cursor and disable head rotation.
- **Notifications:** Activates / Deactivates the different in-game notifications.

<img src="https://github.com/giosuel/imperium/blob/main/assets/imperium-settings.png?raw=true" alt="imperium-settings" style="zoom: 67%;" />

## Oracle Spawn Prediction

Oracle is an algorithm developed by the Imperium team to predict indoor, outdoor and daytime entity spawns over the course of the day.

Oracle predicts daytime, outdoor and indoor entities including their spawn times and spawn positions. Clicking the position text in an oracle entry teleports you to the spawn point.

Entities spawned with Imperium do not count towards current level power or their max count, meaning, they **do not influence** the entity spawning.

> [!NOTE]
>
> Oracle is currently in beta stage and is therefore not guaranteed to always work properly.

![imperium-oracle](https://github.com/giosuel/imperium/blob/main/assets/imperium-oracle.png?raw=true)

### Prediction Restrictions

Entity spawns in Lethal Company are dependent on a lot of factors. Since v47 everything became more strictly dependent on the map seed and while this made things a lot easier, there are still a few exceptions in the code that mess up entity spawning. Oracle accounts for all these changes, meaning the spawn predictions can change if one of the following events occur:

- An entity dies (Affects all spawn cycles).
- An eyeless dog kills an employee.
- A masked (entity) is searching for a place to walk to.
- A forest giant is grabbing a player / reaching it's targeted AI node during roaming.
- A bunker spider attempts to place a web.

While Oracle notifies you for most of these prediction adjustments, due to the frequent nature of some of them (e.g. masked searching for a node) it is just not feasible to spam notifications. Make sure to regularly check the Oracle UI for changes if there are masked or bunker spiders present on the map as the prediction can shift a lot.

## Issue Tracking

Feel free to submit bug reports and feature requests as issues on Imperium's [Github](https://github.com/giosuel/imperium) page. Don't forget to include your BepInEx logs to make my life easier :)

### Known Issues

- Switching from an Imperium UI to UE and back will cause the cursor to be unlocked.
  - **FIX:** Toggle Unity Explorer on and off.
- Navigating to Liquidation will softlock the game and the save has to be reloaded.
  - **FIX:** Restart the game / reload the save file.
- Save file editor doesn't work on a newly created save file.
  - **FIX:** Reload the save file.


## Credits

Imperium was designed and implemented by the Imperium team ([giosuel](https://github.com/giosuel)).

Various icons were provided by [FlatIcon](https://www.flaticon.com/).

### Special Thanks To

- [Nebby](https://thunderstore.io/c/lethal-company/p/Nebulaetrix/) for helping with the documentation and the testing!

- [Swaggies](https://thunderstore.io/c/lethal-company/p/Swaggies), [aminoob](https://thunderstore.io/c/lethal-company/p/aminoob/) and star0138 for the help with the testing of Imperium!
  
- [Dancemoon](https://thunderstore.io/c/lethal-company/p/dancemoon/) who made DanceTools for the inspiration for Imperium!
  
- [chaser324](https://github.com/Chaser324) for providing the wireframe shader for the collider visualizations!

- [Sligili](https://thunderstore.io/c/lethal-company/p/Sligili/) for writing HDLethalCompany!

