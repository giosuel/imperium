# Changelog

## Imperium v0.2.7 [Beta] - V64 Compatibility Patch

**Note:** *This version of Imperium is compatible with the [Version 64 (Beta)](<https://steamdb.info/changelist/25039181/>) and [Version 62](https://steamdb.info/changelist/24877906/) of Lethal Company.*

- Fixed a bug where the player was always targetable when untargetabillity was turned off.
- Removed weed entity spawn prediction from Oracle.
- Fixed a bug where items weren't properly updated when the player teleported.

## Imperium v0.2.6 [Beta] - Hotfix

- Fixed a bug where the infinite turbo option was always enabled for the company cruiser.

## Imperium v0.2.5 [Beta] - Hotfix

- Fixed a bug where the interior would always be the Mineshaft for the host.
- Fixed a bug where the freecam would break spectating.

## Imperium v0.2.4 [Beta] - Bug Fixes

### Internal Fixes

- Fixed a bug where duplicate outside hazards would prevent Imperium from spawning.
- Fixed a bug where the theme wouldn't apply correctly to the freecam sliders.
- Fixed a bug where visualizers would sometimes fail to find an entity.
- Added integration for [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/).
- Changed some theme settings to make colors more consistent across UIs.
- Fixed a bug where respawning entities would not work.
- Various performance tweaks to the object explorer window.
- Fixed some performance issues with Oracle.

## Imperium v0.2.3 [Beta] - Bug Fixes

### Internal Fixes

- Fixed a bug where opening the UI while being moved by the game would crash the game.
- Fixed a bug where despawning the cruiser would break the game for the driver.
- Fixed a bug where incorrect parsing of some input fields would break Imperium startup.
- Fixed a bug where opening the teleport window would teleport you in place.

## Imperium v0.2.2 [Beta] - The Event Log

Besides a load of QoL changes, bug fixes and compatibility changes, this update is aimed at the implementation of the event log and the tape measure. The event log is a tool that allows you to track in-game events such as entity behaviour, targeting and noise detection more easily.

Another change is that Imperium is now usable even if not everyone in the lobby has it installed. There is currently no desync guarding, meaning it is possible to cause desync issues when clients don't have it installed, so use with caution! I will be working on automatically deactivating certain functionality based on the lobby to avoid these issues in the future.

**Note:** *This version of Imperium is compatible with the [Version 60](<https://steamdb.info/changelist/24827461/>) of Lethal Company.*

### Highlights

#### Event Log

The event log allows you to track specific game events such as entity spawns, noise detection, entity targeting and LoS detection and more.

![event-log](https://github.com/giosuel/imperium/blob/main/assets/event-log.png?raw=true)

#### Tape Measure

The tape measure is a virtual tool to measure distances in the game. The tape measure supports axis snapping when holding down the `Alt` key. It also tries to snap to surfaces as well as possible.

![tape-measure](https://github.com/giosuel/imperium/blob/main/assets/tape-measure.png?raw=true)

### Added Stuff

- Added the event log to track specific game events.
- Outdoor map objects (e.g. trees, rocks, pumpkins) can now be spawned, teleported and removed.
- Vain shrouds can now be teleported around and removed from the map.
- Added teleportation to fire exits to the teleportation window.
- Added a virtual tape measure to measure distances in Unity units and meters.
- Added an option to make the player untargetable for entities.
- Added an option to disable all UI tooltips.
- Added visualizers for the Roaming Locusts detection ranges.
- Added visualizers for the Coil-Head detection radius.
- Added an option to control the player's flying speed.
- Added various debug options for the Company Cruiser.
  - Cruiser god-mode option makes the cruiser invincible to damage.
  - Infinite turbo option makes the cruiser have turbo permanently.
  - Instant ignition option ,allows you to always start the cruiser on first try.
  - Sliders for push force and acceleration.
- Added various new default insights to the player and selected entities.
- Added sliders to indicate the current field of view and movement speed when using the freecam.


### Changes

- Rewrote the object explorer lists to use object pooling to support basically an infinite amount of objects on the map with minimal performance impact.
- Teleporting an item now causes it to play the drop SFX which can be detected by entities.

### QoL Improvements

- Min and max zoom icons are now clickable in the map zoom slider.
- Replaced the icon of the preferences window with a more suitable one.
- Reordered the dock buttons to improve intiution.
- Added various tooltips to help simplify the navigation of the UI.
- Improved teleport indicator navigation.

### Internal Fixes

- Fixed a bug where the weather wouldn't update on the ship's screen when in orbit.
- Fixed a bug where the player would keep bleeding when activating god mode.
- Fixed a bug where the damage UI overlay wouldn't go away when activating god mode.
- Improved the first-person visualizer tracking of the shotgun visualizer by digger1213.
- Upgraded [LethalNetworkAPI](<https://thunderstore.io/c/lethal-company/p/xilophor/LethalNetworkAPI/>) from v2 to v3.
- Fixed a bug where scrolling would result in error spam in the logs.
- Added fallback to Imperium startup if errors occur.
- Added interval timing to Insight generator functions to greatly increase performance.
- Fixed a problem with the zoom slider in the map interface.
- Fixed problems with the factory reset functionality.
- Fixed a bug where items would fall through the ground when spawned via freecam.
- Imperium's assets are now bundled with the DLL, simplifying debugging.
- Fixed the spawn report to actually log spawned entities.
- Fixed the Baboon Hawk's LoS visualizers.
- Fixed an Oracle problem related to the spawning of weed entities.
- Fixed a bug where the ship ambience noise would be sped up.
- Fixed a bug where spider webs would be indestructible after the spider has died.
- Fixed a bug where mods that add entities with special characters in their name would interfere with Imperium's launch.
- Fixed a bug where the freecam's field of view would get stuck.

## Imperium v0.2.1 [Beta] - V56 Compatibility Patch

A hotfix to allow support for the released v56 version of Lethal Company.

**Note:** *This version of Imperium is **only** compatible with the [Version 56](<https://steamdb.info/changelist/24262549/>) of Lethal Company.*

### Internal Fixes

- Added compatibility for the new v56 patch ([24261494](<https://steamdb.info/changelist/24262549/>)).
- Fixed a bug where setting the weather to "None" would break the game.
- Updated Oracle Spawn Prediction to account for the bugfixes and weed enemies.

## Imperium v0.2.0 [Beta] - The Interface Update

This update is a huge one and mainly aimed at the rework of the UI system as well as the integration of new visualizers and debug options for the new update v55! As the previous UI solution was quite static and hard to maintain and expand, I decided to switch from static views with frozen windows to a more dynamic approach with floating windows! I also decided to finally integrate [InputUtils](<https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/>), as a lot of people have requested. Imperium keybinds can now be re-bound in the settings.

Besides that, I re-wrote the whole internal networking and switched form manual RPCs to using the [LethalNetworkAPI](<https://github.com/Xilophor/LethalNetworkAPI>) for all network communication in Imperium. This should provide more stability when using Imperium in multiplayer and reduce client de-syncs when changing game variables at runtime.

Finally, I also expanded the Imperium API and added a lot of new functionality, including being able to synchronously spawn items and entities as well as enabling and disabling core functionality such as god mode or flight.

### Added Stuff

- Merged most of the smaller windows into a large Imperium UI that supports floating windows.
  - Windows can be dragged and resized with the mouse cursor.
  - Holding `Alt` and dragging an window results in the window being resized.

- Added tooltips to several buttons and Imperium settings to serve as in-game help with the interface.
- Added [InputUtils](<https://thunderstore.io/c/lethal-company/p/Rune580/LethalCompany_InputUtils/>) integration to make keybinds changeable in the settings.
- Merged the navigator window and ship settings into a new Ship Control UI.
- Moon settings were merged with the moon control center into a new Moon Control UI.
- The teleport UI and the waypoint manager were merged into a new Teleportation UI.
- Added support for modded weathers in the Moon Control UI.
- Added scrap, entity and map hazard spawning functions to the Imperium API.
- Added steam valves as new map hazard in the object explorer and spawn UI.
- Added new custom visualizer for Nutcrackers.
- Added new custom visualizer for Hoarding Bugs.
- Added an option to permanently enable the drunk effect from the TZP-Inhalant.
- Added new custom visualizer for the Kidnapper Fox.
- Added an option to spawn Vain Shrouds from the SpawningUI.
- Added a new visualizer for Vain Shrouds.
- Added a new visualizer for Vain Shroud attraction points.
- Added an option to spawn the Company Cruiser from the SpawningUI.
- Added default insights for the Company Cruiser.
- Added a slider to change the push control for the Company Cruiser.

### Changes

- Small Object Explorer functionality rework.
  - Toggling objects in the object explorer is now synced with other clients.
  - Disabling entities now results in them being frozen instead of deactivated.
  - Toggling turrets and landmines now results in them being enabled / disabled as if an employee would do it from the terminal instead of the object itself being enabled / disabled.
  - Toggling breaker boxes now results in all the switches being flipped instead of the object itself being  enabled / disabled.
  - Toggling steam valves will burst / repair them.
- Merged the amazing shotgun visualizer rework by [digger1213](<https://github.com/digger1213>).
- Pausing time is now possible from space.

### QoL Improvements

- Adjusted all themes to better match the new overlapping window style.
- Modded weather and moons now show up correctly in the respective UIs.
- Freecam flight controls were changed to match with the creative flying controls.
  - Default: `Ctrl` -> Descend, `Space` -> Ascend, `Return` -> Toggle the selected layer.
- The zoom slider in the Map UI now uses a logarithmic scale.
- Various small changes to the Oracle UI including new formatting of the vectors.
- Added a way to highlight entity ghost spawns for indoor entities (Caused by a bug in the game).
- Disabling the flying option now disables flight.
- Added aliases for Insight class names to simplify class identification.
- Changed the trigger, collider and navmesh surface visualizer's material.

### Internal Fixes

- Fixed a bug where it was not possible to join an Imperium lobby without having Imperium installed.
- Fixed the animation skipping options `Interact` and `InteractHold`.
- Fixed a typo in a function signature in the Imperium API.
- Fixed a bug where players could die in orbit when god mode was off.
- Changed it so insight generators are executed in `LateUpdate` for consistency.
- Fixed player invisibility and made it more consistent across all entities.
- Fixed a bug where the ship would always land instantly, even after disabling the option.
- Fixed a bug where Nutcrackers were affected by infinite ammo and full auto shotgun.
- Added various functions to the log silencer feature of Imperium.

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