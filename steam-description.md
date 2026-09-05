[h1]Description[/h1]
Better Pawn Control Forked adds named presets for colony policies. Save different assignments, schedules, work priorities, areas, animal settings, mech settings, and supported loadouts, then switch between them from RimWorld's management tabs.

This maintained fork targets RimWorld 1.6 and adds defensive handling for heavily modded colonies. Version 2.9.1 also keeps assignment changes made through Health, Numbers, imHUD, and similar interfaces until you explicitly select a BPC preset.

[b]Use the original Better Pawn Control or this fork, not both.[/b]

[h1]Features[/h1]
[list]
[*]Presets for outfits, food, drugs, reading, medicine, hostility response, schedules, allowed areas, and work priorities.
[*]Work Tab extended priorities and custom work-type handling when Work Tab is available.
[*]Animal, mech, robot, and supported loadout presets.
[*]Default policies for new colonists, prisoners, and slaves.
[*]An emergency toggle and keybind for applying configured presets together.
[*]Schema migration for original Better Pawn Control and pre-2.9 fork saves.
[*]Policy and pawn-link transfers between maps, including gravship moves.
[*]Optional Progression: Education and Outfit Stands Plus Forked support.
[/list]

[h1]How to Use[/h1]
[list=1]
[*]Open the Assign, Schedule, Work, Animals, Mechs, or Weapons tab.
[*]Select the BPC cog to create or manage presets for that tab.
[*]Choose a preset with the policy button beside the cog.
[*]Change RimWorld's normal controls while the preset is active.
[*]Select a BPC preset whenever you want to apply its saved values.
[/list]

Opening the Assign tab alone does not reapply saved values. Preset application happens when you select a BPC preset.

[h1]Settings and Configuration[/h1]
Use RimWorld's mod settings for defaults, emergency behavior, and optional integrations. Use the BPC cog menus in the supported management tabs to manage presets.

[h1]Requirements and Dependencies[/h1]
[list]
[*]RimWorld 1.6.
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077]Harmony[/url].
[/list]

[h1]Compatibility, Load Order, and Save Migration[/h1]
[list]
[*][b]Original mod:[/b] Do not enable Better Pawn Control and Better Pawn Control Forked together. The packages are marked incompatible, and the fork disables its patches if both are forced active.
[*][b]Load order:[/b] Load Harmony first. The mod metadata places this fork after supported integration mods when required.
[*][b]Optional integrations:[/b] Work Tab, Combat Extended, Compositable Loadouts, FSF Complex Jobs, Progression: Education, Defensive Positions Forked, and Outfit Stands Plus Forked.
[*][b]Save migration:[/b] Version 2.9 and later migrate original-mod and older-fork data to schema 2.
[*][b]Removed definitions:[/b] Missing work types and work givers are skipped without blocking the rest of a preset. Temporarily unavailable optional-mod records are retained.
[/list]

[h1]Fork History[/h1]
Better Pawn Control was created by VouLT. The original supports RimWorld 1.6; this fork is maintained for mod lists that need more defensive handling around custom work types, nonstandard pawn trackers, schedules, optional integrations, and apparel jobs.

Compared with the original, this fork adds capability-based pawn handling, resilient work and schedule data, stable policy-ID migration, gravship transfer fixes, Progression: Education support, and guarded Outfit Stands Plus Forked integration. If the original works for your mod list, you do not need to switch.

[h1]Credits[/h1]
Original Better Pawn Control by VouLT. This fork is maintained by KyleMHB. Original contributors and translators remain credited for the project this fork is based on.

Progression: Education support is based on Machado's BetterPawnControl ProgressionEducation Patch.

[h1]License and Forking Policy[/h1]
This fork inherits the original Better Pawn Control MIT license.

If your fork primarily consists of bug fixes or feature additions that align with the core vision of this mod, I reserve the right to request that your changes be submitted as a Pull Request to my existing codebase rather than being published as a completely separate standalone release.

This is a project request, not an additional restriction on the MIT license.

[h1]Links[/h1]
Support me on Ko-fi. This does not imply endorsement by the original authors.

[url=https://ko-fi.com/I7L525WMJ6][img]https://img.shields.io/badge/Support_me_on_Ko--fi-72a4f2?style=for-the-badge&logo=kofi&logoColor=white[/img][/url]
[url=https://github.com/KyleMHB/BetterPawnControlForked][img]https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github&logoColor=white[/img][/url]
[list]
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=3724294345]Better Pawn Control Forked on Steam Workshop[/url]
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=1541460369]Original Better Pawn Control on Steam Workshop[/url]
[*][url=https://github.com/voult2/BetterPawnControl]Original source repository[/url]
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=3673605975]BetterPawnControl ProgressionEducation Patch[/url]
[/list]
