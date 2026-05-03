# Better Pawn Control Forked

Better Pawn Control Forked is a RimWorld quality-of-life mod for switching colony policies in bulk. It lets you save and swap policy presets for colonists, animals, mechs, work, schedules, assignments, and supported weapon or loadout integrations.

This fork focuses on RimWorld 1.6 compatibility and safer interaction with other pawn-control and gear-management mods.

## Features

- **Policy presets** for outfits, food, drugs, reading, medicine, hostility response, and optional medicine inventory stock.
- **Schedule presets** including area restrictions.
- **Work presets** with optional Work Tab inner-priority support.
- **Animal, mech, robot, and loadout presets** for supported integrations.
- **Default policies** for new colonists, prisoners, and slaves.
- **Emergency toggle** for quickly applying configured policy sets.
- **Integration-aware apparel handling** for Outfit Stands Plus Forked.
- **Capability-based compatibility checks** for DLC and modded pawn setups, including nonstandard humanlike pawns with partial trackers.
- **Optional schedule compatibility** for Progression: Education class assignments.
- **Worktype-safe work policy handling** for Work Tab, FSF Complex Jobs, and other worktype overhauls.

## Installation

### Steam Workshop

Subscribe on Steam Workshop and enable **Harmony** and **Better Pawn Control Forked** in RimWorld's mod list.

### Manual Installation

1. Download or clone the repository.
2. Place the mod folder in your RimWorld `Mods` directory.
3. Enable **Harmony** and **Better Pawn Control Forked**.

## Usage

1. Open the relevant RimWorld tab, such as Assign, Schedule, Work, Animals, Mechs, or Weapons.
2. Click the BPC cog button to create or manage policies.
3. Select a policy from the BPC policy button.
4. Configure the normal RimWorld policy controls while that BPC policy is active.
5. Switch between BPC policies when you want those saved settings reapplied.

## Configuration

Configuration is stored in RimWorld's normal mod settings and the in-game policy editors.

Key settings and supported behaviors include:

- default policies for new pawns
- emergency policy switching
- optional Work Tab integration
- optional animal, mech, robot, and loadout integrations
- optional Progression: Education schedule syncing
- capability-aware handling for DLC and modded pawns
- Outfit Stands Plus Forked apparel policy triggering

## Building from Source

Prerequisites:

- .NET SDK compatible with the project
- RimWorld reference files configured for the project

Build the 1.6 assembly from the repository root:

```powershell
dotnet build .\Source\BetterPawnControlForked.csproj -c Release /p:UseSharedCompilation=false
```

The compiled DLL is copied manually into `v1.6/Assemblies/BetterPawnControlForked.dll` when preparing a local mod package.

## Testing and Validation

The primary validation command is the Release build:

```powershell
dotnet build .\Source\BetterPawnControlForked.csproj -c Release /p:UseSharedCompilation=false
```

## Contributing & Forking Policy

> Contributions, issues, and feature requests are welcome.
>
> **Forking Policy:** If your fork primarily consists of bug fixes or feature additions that align with the core vision of this project, I reserve the right to request that your changes be submitted as a Pull Request to this existing codebase rather than being published as a completely separate standalone release, package, listing, or distribution.

## Links

- **Steam Workshop:** <http://steamcommunity.com/sharedfiles/filedetails/?id=1316142788>

## License

> This project is a fork and inherits the original project's license. See the original project for license terms: <https://github.com/voult2/BetterPawnControl>.

## Credits

- VouLT for the original Better Pawn Control mod.
- Fluffy for Animal Tab integration help.
- Skyarkhangel for Combat Realism integration support.
- Marnador for the logo font.
- Lauri7x3, Coldmoon, boundir, Proxyer, 53N4, Crusader, Deno226, Ionfrigate, and others for translations and updates.
- debugzxcv, muggenhor, TheLonerD, DomB, and other contributors for integration and compatibility work.
- Machado for the BetterPawnControl ProgressionEducation Patch: <https://steamcommunity.com/sharedfiles/filedetails/?id=3673605975>.





