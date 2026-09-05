# Better Pawn Control Forked

Better Pawn Control Forked adds named presets for RimWorld colony policies. Save different assignments, schedules, work priorities, areas, animal settings, mech settings, and supported loadouts, then switch the colony between them from the relevant management tab.

This maintained fork targets RimWorld 1.6 and adds defensive handling for heavily modded colonies, optional integrations, save migration, and map transfers.

## Features

- Save and switch outfit, food, drug, reading, medicine, hostility-response, and medicine-stock settings.
- Save schedules and allowed areas together.
- Save work priorities, including Work Tab's extended priorities when available.
- Manage supported animal, mech, robot, and loadout policies.
- Assign defaults for new colonists, prisoners, and slaves.
- Apply selected presets together with the emergency toggle or keybind.
- Keep policy data for temporarily unavailable optional mods so it can return when their definitions are restored.
- Move pawn links and active policy selections with gravships and other map transfers.
- Support compatible DLC and modded humanlike pawns according to the trackers they provide.

## Installation

### Steam Workshop

1. Subscribe to [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) and [Better Pawn Control Forked](https://steamcommunity.com/sharedfiles/filedetails/?id=3724294345).
2. Enable Harmony before Better Pawn Control Forked in RimWorld's mod list.
3. Do not enable the original Better Pawn Control at the same time.

### Manual installation

1. Download a release and extract the mod folder into RimWorld's `Mods` directory.
2. Enable Harmony before Better Pawn Control Forked.
3. Restart RimWorld after changing the active mod list.

## Usage

1. Open a supported management tab: Assign, Schedule, Work, Animals, Mechs, or Weapons.
2. Select the BPC cog to create, rename, reorder, or remove presets.
3. Use the policy button beside the cog to choose a preset.
4. Change RimWorld's normal controls while that preset is active. BPC records those changes when you leave the window or switch presets.
5. Select a BPC preset when you want to apply its saved values.

Opening the Assign tab does not reapply a saved preset. This lets changes made through Health, Numbers, imHUD, and similar interfaces remain in place until you explicitly select a BPC preset.

## Settings and configuration

RimWorld's mod settings contain the default and integration options. The cog menus in each supported management tab control the presets for that tab.

You can configure:

- default policies for new colonists, prisoners, and slaves;
- the presets used by emergency mode;
- Work Tab integration;
- optional animal, mech, robot, and loadout integrations;
- Progression: Education schedule syncing; and
- Outfit Stands Plus Forked apparel handling.

## Compatibility and save migration

- Requires RimWorld 1.6 and Harmony.
- The original Better Pawn Control and this fork are incompatible. The fork disables its Harmony patches if both are forced active.
- Optional integrations include Work Tab, Combat Extended, Compositable Loadouts, FSF Complex Jobs, Progression: Education, Defensive Positions Forked, and Outfit Stands Plus Forked.
- Version 2.9 and later migrate original Better Pawn Control and pre-2.9 fork data to schema 2.
- Missing work types and work givers are skipped without preventing the rest of a preset from loading.

## Building from source

Install the .NET 8 SDK, then run this command from the repository root:

```powershell
.\deploy.ps1 -Version 2.9.1 -Configuration Release
```

The script restores pinned references, builds the RimWorld 1.6 assembly, runs the automated tests, validates the package, and creates `artifacts/BetterPawnControlForked-2.9.1.zip`. It does not install or publish the package.

## Testing and validation

The packaging command runs the 11 core tests. Run the compiled Assign-tab regression separately:

```powershell
.\Tests\AssignTabOpenRegressionTests.ps1
```

See [TESTING.md](TESTING.md) for the compatibility and in-game test matrix.

## Fork history and credits

Better Pawn Control was created by VouLT. This fork keeps the original workflow while adding RimWorld 1.6 maintenance, safer optional integrations, schema migration, policy-ID persistence, and map-transfer fixes.

Additional credits:

- Fluffy for Animal Tab integration help.
- Skyarkhangel for Combat Realism integration support.
- Marnador for the logo font.
- Lauri7x3, Coldmoon, boundir, Proxyer, 53N4, Crusader, Deno226, Ionfrigate, and other translators and contributors.
- debugzxcv, muggenhor, TheLonerD, DomB, and other integration and compatibility contributors.
- Machado for the [BetterPawnControl ProgressionEducation Patch](https://steamcommunity.com/sharedfiles/filedetails/?id=3673605975), which the Progression: Education support is based on.

## Links

Support me on Ko-fi. This does not imply endorsement by the original authors.

[![Support me on Ko-fi](https://img.shields.io/badge/Support_me_on_Ko--fi-72a4f2?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/I7L525WMJ6)
[![GitHub Repository](https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/KyleMHB/BetterPawnControlForked)

- [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3724294345)
- [Original Better Pawn Control on Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=1541460369)
- [Original source repository](https://github.com/voult2/BetterPawnControl)

## Contributing and Forking Policy

> Contributions, issues, and feature requests are welcome.
>
> **Forking Policy:** If your fork primarily consists of bug fixes or feature additions that align with the core vision of this project, I reserve the right to request that your changes be submitted as a Pull Request to this existing codebase rather than being published as a completely separate standalone release, package, listing, or distribution.

## License

This fork inherits the original Better Pawn Control MIT license. See [LICENSE](LICENSE) and the [original project](https://github.com/voult2/BetterPawnControl) for the applicable terms and attribution.
