[h1]Description[/h1]
Better Pawn Control Forked lets you save and switch colony policy presets for outfits, food, drugs, medicine, schedules, work priorities, allowed areas, animals, mechs, and supported loadout mods.

[b]Version 2.9.0[/b] automatically migrates presets from the original Better Pawn Control and older fork saves. It also preserves optional integration data when a mod is temporarily unavailable, fixes gravship policy transfers, and hardens pawn lifecycle handling for guests, prisoners, recruits, and slaves.

[b]Use the original mod or this fork, not both.[/b] The packages are marked incompatible, and this fork disables its Harmony patches if both are forced active.

[h1]Features[/h1]
[list]
[*]Presets for outfits, food, drugs, reading, medicine, hostility response, schedules, allowed areas, and work priorities.
[*]Optional support for Work Tab extended priorities and custom work types.
[*]Animal, mech, robot, and supported loadout presets.
[*]Default policies for new colonists, prisoners, and slaves.
[*]An emergency button and keybind for applying configured presets at once.
[*]Capability-based handling for DLC and modded humanlike pawns with nonstandard trackers.
[*]Optional integration with Progression: Education and Outfit Stands Plus Forked.
[/list]

[h1]How to Use[/h1]
[list=1]
[*]Open the Assign, Schedule, Work, Animals, Mechs, or Weapons tab.
[*]Click the BPC cog to create or manage presets.
[*]Select a preset with the button beside the cog.
[*]Change the normal RimWorld settings while that preset is active.
[*]Select another preset when you want BPC to apply its saved settings.
[/list]

[h1]Settings and Configuration[/h1]
Use RimWorld's mod settings and the BPC controls in the supported game tabs to configure defaults, emergency policies, optional integrations, and preset behavior.

[h1]Requirements and Dependencies[/h1]
[list]
[*]RimWorld 1.6.
[*][url=https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077]Harmony[/url].
[/list]

[h1]Compatibility, Load Order, Multiplayer, and Save Safety[/h1]
[list]
[*][b]Load order:[/b] Load after Harmony and supported integration mods when applicable.
[*][b]Custom work types:[/b] Missing work types and work givers are skipped without stopping the rest of a preset from loading.
[*][b]Optional integrations:[/b] Includes defensive failure handling for Work Tab, Combat Extended, Compositable Loadouts, Progression: Education, FSF Complex Jobs, Defensive Positions Forked, and Outfit Stands Plus Forked.
[*][b]Save migration:[/b] Version 2.9.0 migrates original-mod and older-fork policy data to the current schema.
[*][b]Gravships and map moves:[/b] Pawn links and active policy selections move together.
[/list]

[h1]Fork History[/h1]
Better Pawn Control was created by VouLT. The original already supports RimWorld 1.6; this fork is intended for heavily modded games that need more defensive handling around custom work types, nonstandard pawn trackers, schedules, optional integrations, and apparel jobs.

Compared with the original, this fork adds capability-based pawn handling, resilient work and schedule serialization, stable policy-ID migration, gravship transfer fixes, Progression: Education support, and guarded Outfit Stands Plus Forked integration. If the original works for your mod list, you do not need to switch.

[h1]Credits[/h1]
Original Better Pawn Control by VouLT. This fork is maintained by KyleMHB. Original contributors and translators remain credited for the project this fork is based on.

Progression: Education support is based on Machado's BetterPawnControl ProgressionEducation Patch.

[h1]License and Forking Policy[/h1]
This fork inherits the original Better Pawn Control project's MIT license.

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
