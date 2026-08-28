[h1]Better Pawn Control Forked[/h1]

Better Pawn Control lets you save sets of colony policies and switch between them. You can use it for outfits, food, drugs, medicine, schedules, work priorities, allowed areas, animals, mechs, and supported loadout mods.

[h1]2.9.0 save migration and gravship fixes[/h1]

Version 2.9 automatically imports presets from the original Better Pawn Control and older fork saves. Work types, work givers, weapon presets, robot presets, and loadout data remain saved when an optional mod is temporarily unavailable.

Pawn lifecycle handling is null-safe for recruitment, guests, prisoners, and slaves. Gravship moves now carry both pawn links and the active preset selection. Emergency presets use stable policy IDs and safely return to policy 0 if a selected preset was deleted.

[b]Do not enable the original and fork together.[/b] The mod list marks them incompatible, and the fork disables its Harmony patches if both are forced active.

[h1]So why is there a fork?[/h1]

First, to clear up some confusion: [b]the original Better Pawn Control works on RimWorld 1.6.[/b] This is not an unofficial 1.6 update.

I made this fork for a heavily modded game where I needed BPC to be less strict about what counts as a normal colonist, and less likely to fall over when another mod changes work types, pawn trackers, schedules, or apparel jobs.

If the original works for your mod list, there is no reason you have to switch. The useful differences in this fork are the following:

[list]
[*][b]FSF Complex Jobs and custom work types[/b]

The original Workshop page lists Complex Jobs and mods that expand jobs as incompatible. This fork only saves work types that are valid for the pawn, skips missing work types and work givers, and carries on if one of Work Tab's extended priorities cannot be read. One bad or removed entry should not stop the rest of the preset from loading.

[*][b]Modded and DLC pawns[/b]

Some humanlike pawns do not have all the trackers that a normal colonist has. Instead of assuming every pawn supports every BPC feature, this fork checks for the outfit, food, drug, reading, schedule, work, area, and medicine inventory trackers separately. A pawn is included in the parts it can actually use and skipped in the others.

It also does not remove a valid modded pawn from a saved preset just because the pawn is not classed as a vanilla colonist.

[*][b]Removed policies and schedule definitions[/b]

Policies and schedule entries can disappear when mods are removed or changed. Where possible, this fork keeps the pawn's current valid policy instead of immediately replacing it with a default. Schedule entries are stored by name, and incomplete schedules or missing entries are repaired while loading.

[*][b]Outfit Stands Plus Forked[/b]

When a BPC outfit preset changes, the fork can send a pawn to their assigned outfit stand. It only does this if the stand is reachable and contains an outfit the pawn can wear under the new policy.

It will not start the outfit stand job while the pawn already has a Defensive Positions, Gear Up And Go, wear-apparel, or remove-apparel job active or queued. It also leaves locked apparel alone.

[*][b]Progression: Education[/b]

Machado's Progression: Education compatibility patch is included in the fork. Class timetable changes are copied into BPC's default schedule preset. Switching to a different BPC schedule clears class-only assignments from the pawn's current timetable.

[*][b]A few smaller fixes[/b]

Schedule changes no longer call the pawn's tick method directly. The fork also handles missing pawn trackers in more places and avoids repeated player-faction errors during startup and early map loading.
[/list]

[b]Use the original or this fork, not both together.[/b]

[h1]Features[/h1]

[list]
[*]Presets for outfits, food, drugs, reading, medicine, hostility response, schedules, allowed areas, and work priorities
[*]Optional support for Work Tab's extended priorities
[*]Animal, mech, robot, and supported loadout presets
[*]Default policies for new colonists, prisoners, and slaves
[*]An emergency button and keybind for switching your configured presets at once
[/list]

[h1]How to use it[/h1]

[list=1]
[*]Open the Assign, Schedule, Work, Animals, Mechs, or Weapons tab.
[*]Click the BPC cog to create or manage presets.
[*]Select a preset with the button next to the cog.
[*]Change the normal RimWorld settings while that preset is active.
[*]Select another preset when you want BPC to apply its saved settings.
[/list]

[h1]Requirements[/h1]

[list]
[*]RimWorld 1.6
[*]Harmony
[/list]

The fork contains specific compatibility work for FSF Complex Jobs, Progression: Education, Defensive Positions Forked, and Outfit Stands Plus Forked.

[h1]Credits and source[/h1]

This is a fork of [url=https://steamcommunity.com/sharedfiles/filedetails/?id=1541460369]Better Pawn Control[/url] by VouLT. It inherits the original project's license, and the original contributors and translators deserve credit for the mod this is based on.

Progression: Education support is based on Machado's [url=https://steamcommunity.com/sharedfiles/filedetails/?id=3673605975]BetterPawnControl ProgressionEducation Patch[/url].

[list]
[*][url=https://github.com/KyleMHB/BetterPawnControlForked]Fork source and issue tracker[/url]
[*][url=https://github.com/voult2/BetterPawnControl]Original source[/url]
[/list]

[h1]Forking policy[/h1]

If your fork mainly contains bug fixes or features that fit this project's purpose, I may ask you to submit those changes as a pull request instead of publishing another separate version.
