# Changelog

## Imperium 0.1.6 [Beta] - The Map

Imperium now has its own map and minimap!

Besides that, this update is mainly aimed at QoL fixes and changes. I reworked the night vision system to be more uniform and comfortable to use.

### Compatibility

This version is compatible with the [v50](https://steamdb.info/changelist/23181066/) update of Lethal Company.

### Changes

- Added an option to prevent the ship from leaving automatically.
- Reworked night vision to be more uniform and comfortable to use.
- The custom welcome message can now be turned off in the settings. 

### Internal Fixes

- Security doors now correctly open and close.
- Fixed a bug where changing weathers was causing an exception on certain moons.
- Fixed the freecam teleport button in the Teleport UI.

### QOL Changes

- Teleport Indicator can now be toggled with **T**.

## Imperium 0.1.5 [Beta]

I re-wrote the entity and item management system to improve the compatibility with mods that add items and entities with [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/).

In general, using Imperium in existing v50 modpacks should now work a lot better, but I still can't guarantee that it doesn't break with certain mods.

### Compatibility

This version is compatible with the [v50](https://steamdb.info/changelist/23181066/) update of Lethal Company.

#### Known Mod Incompatibilities

- [NutcrackerFixes](https://thunderstore.io/c/lethal-company/p/Zaggy1024/NutcrackerFixes/) by Zaggy1024 - Breaks F-key menus
- [Minimap](https://thunderstore.io/c/lethal-company/p/Tyzeron/Minimap/) by Tyzeron - Conflicts with the cursor locking

### Changes

- Completely rewrote the Spawning UI and entity handling system to be more robust.
- Added support for modded items and entities.
- Items, scrap and spider webs can now be teleported from the Object Explorer.
- Added indoor spawning vents to the Object Explorer.
- Added breaker boxes to the Object Explorer.

### Internal Fixes

- Changing the weather in real-time is now fully implemented.
- FOV transitions are now smooth (e.g. when sprinting).
- Fixed a bug that caused freecam inputs to go through when the UI was open.
- Fixed the BepInEx dependency.
- Fixed a bug that caused problems when spawning entities with a space in their name.

### QOL Changes

- Entity spawn location can now be picked interactively when in freecam.
- Freecam now spawns above the player when opened for the first time.
- More overlays now correctly look at freecam when its active.

## Imperium 0.1.4 [Beta]

### Compatibility

This version is compatible with the [v50 rev.4](https://steamdb.info/changelist/23125974/) update of Lethal Company.

### Added Features

- Added picture-in-picture mode for freecam (Shortcut: **X**).
- Added keybind to toggle the Player HUD (Shortcut: **Z**).
- Added visualization of the outdoor spawn denial points.
- Added night vision to the freecam.
- Added noise markers to indicate nearby noises.

### Internal Fixes

- Switched from runtime netcode patching to [post-build patching](https://github.com/EvaisaDev/UnityNetcodePatcher).

### QOL Changes

- Dead entities are now marked as dead in the object explorer.

## Imperium 0.1.3 [Beta]

### Compatibility

This version is compatible with the [v50 rev.3](https://steamdb.info/changelist/23055571/) update of Lethal Company.

### Buxfixes

- Adjusted Oracle predictions as the daytime entity bug was fixed.
- Updated README with new images.