# Changelog

## Imperium v0.1.9 [Beta] - The API Update

As so many of you have requested more advanced debugging functionality that can be accessed by other mods, I have started to implement an API that can be used either in the UE console or by other mods.

I also started writing a wiki that should cover the basics on how to use Imperium as well as information for devs that want to work with Imperium or contribute to Imperium.

### Added Stuff

- Added the Imperium API and a [wiki](https://github.com/giosuel/imperium/wiki) for it.
- Added minecraft-like creative flight.
- Added options for the ship to instantly land, take off and travel betwen planets.
- Added an option that removes the hold time on interact triggers (e.g. ship lever or main entrance).
- Added new custom visualizer for Old Birds.
- Added new custom visualizer for Baboon Hawks.
- Added a moon indicator to the minimap.

### QOL Changes

- I had to limit the possible quota deadline to 3 days as deadlines above 4 were actually breaking the game. Instead, I added an option to completely disable the quota deadline for testing.
- Turning on god mode now restores health to full and removes the damage overlay.

### Internal Fixes

- Fixed a bug where spawned entities would sometimes be placed below the ground.

### Known Issues

- Switching between Unity Explorer and a Imperium UI directly breaks the cursor.
  - To fix, just open and close Unity Explorer once.

- Scanner overlays are broken at higher texture resolutions.

- There are currently some issues with multiplayer. Mostly, when someone without Imperium is trying to join an Imperium lobby.

### Compatibility

This version is compatible with the [v50](https://steamdb.info/changelist/23181066/) update of Lethal Company ([Imperium Mod Compatibilities](https://docs.google.com/spreadsheets/d/1DR3VFAv5toT6UEv1PLRXMWODpXgcdFucxhm3qVJAyxA/edit#gid=0)).

## Imperium v0.1.8 [Beta] - The Visualizer Update

I reworked the whole visualizer system and added a lot of new visualizers and indicators, including indicators for entity LOS and noise detection.

There is a new UI from which all visualizers can be managed!

### Added

- Added many new visualizers for various game objects and layers.
- Added a new notification type for access control and spawning notifications.
- Added a new interface to manage all the visualizers in one place.
- Added entity LOS, proximity and noise detection visualizations.
- Added an option to disable out-of-bounds triggers.
- Added an option to unlock all unlockable items in the terminal shop.
- Added a scaling option for the minimap in the minimap settings.

### QOL Changes

- Entity and Player info panels are now rendered in screen-space, resulting in higher resolution.
- The size of the visualizers for indoor and outdoor nodes has been decreased.
- Clamped the freecam movement speed between 1 and 200.

### Internal Fixes

- Fixed a bug where Imperium client access would lock the host out.
- Fixed a bug where the render pipeline changed the aspect ratio of some screens.
- Fixed an issue where not all notifcations were toggleable.
- Fixed a bug where the map camera clipping would not adjust when in unlocked mode.

### Known Issues

- Switching between Unity Explorer and a Imperium UI directly breaks the cursor.
  - To fix, just open and close Unity Explorer once.

- Scanner overlays are broken at higher texture resolutions.

### Compatibility

This version is compatible with the [v50](https://steamdb.info/changelist/23181066/) update of Lethal Company. ([Imperium Mod Compatibilities](https://docs.google.com/spreadsheets/d/1DR3VFAv5toT6UEv1PLRXMWODpXgcdFucxhm3qVJAyxA/edit#gid=0))

## Imperium v0.1.7 [Beta] - Hotfix

### Internal Fixes
- Removed unnecessary debug statements.
- Fixed a bug where items would fall through the ground when teleporting them.
- Fixed a bug where the version wouldn't display correctly.
- FIxed a big where spawned items would fall through the ground.

## Imperium v0.1.6 [Beta] - The Map Update

Imperium now has its own isometric map and minimap!

Besides that, Imperium's UI got a complete overhaul with new components, better and more consistent alignment and brand new interface skins! There are currently 8 different interface skins to pick from.

I also reworked the night vision system to be more uniform and comfortable to use.

### Compatibility

This version is compatible with the [v50](https://steamdb.info/changelist/23181066/) update of Lethal Company ([Imperium Mod Compatibilities](https://docs.google.com/spreadsheets/d/1DR3VFAv5toT6UEv1PLRXMWODpXgcdFucxhm3qVJAyxA/edit#gid=0)).

### Added

- Added a hybrid 2D/3D isometric map - Open with **F8**.

  - Added minimap - Open with **M**.

  - **LMB** - Look, **RMB** - Move, **MMB** - Zoom, **R** - Reset

- Added an option to prevent the ship from leaving automatically.

- Added eight new interface skins to pick from.

- Added option for host to allow or disallow Imperium commands from clients.

- Added visualization for interact triggers.

### Changes

- Completely overhauled the look and feel of the Imperium interface.
- Reworked night vision to be more uniform and comfortable to use.
- The custom welcome message can now be turned off in the settings.

### Internal Fixes

- **Multiplayer works again!**
- Security doors now correctly open and close.
- Fixed a bug where changing weathers was causing an exception on certain moons.
- Fixed the freecam teleport button in the Teleport UI.
- Fixed a bug where Oracle crashed when a vent was overwritten.

### QOL Changes

- Teleport Indicator can now be toggled with **T**.
- The "Vent" category in the object explorer can now be collapsed properly.

### Known Issues
- Switching between Unity Explorer and a Imperium UI directly breaks the cursor.
  - To fix, just open and close Unity Explorer once.

- Scanner overlays are broken at higher texture resolutions.

## Imperium v0.1.5 [Beta] - The Compat Update

I re-wrote the entity and item management system to improve the compatibility with mods that add items and entities with [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/).

In general, using Imperium in existing v50 modpacks should now work a lot better, but I still can't guarantee that it doesn't break with certain mods.

### Compatibility

This version is compatible with the [v50](https://steamdb.info/changelist/23181066/) update of Lethal Company.

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

#### Known Mod Incompatibilities

- [Minimap](https://thunderstore.io/c/lethal-company/p/Tyzeron/Minimap/) by Tyzeron - Conflicts with the cursor locking

## Imperium v0.1.4 [Beta] - The Noise Update

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

## Imperium v0.1.3 [Beta] - Hotfix

### Compatibility

This version is compatible with the [v50 rev.3](https://steamdb.info/changelist/23055571/) update of Lethal Company.

### Buxfixes

- Adjusted Oracle predictions as the daytime entity bug was fixed.
- Updated README with new images.