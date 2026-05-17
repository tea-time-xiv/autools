# Autools

A Dalamud plugin that bundles quality-of-life automation features for Final Fantasy XIV.

## Features

### Auto Priority Aetheryte Pass

Automatically uses a Priority Aetheryte Pass when you are in the overworld and do not already have a teleport-cost reduction buff active (Priority Pass or FC reduced rates).

- Skipped inside duties (BoundByDuty/56/95) and PvP zones.
- 30-second cooldown between attempts.
- Inventory is polled at most once per second.

### No Jog

Automatically cancels the "Jog" buff that the game auto-applies after Sprint expires. Skipped inside duties.

## Commands

- `/passauto` — Toggle Auto Priority Aetheryte Pass on/off
- `/nojog` — Toggle No Jog on/off

Both can also be toggled from the plugin main window or the configuration window.

## Installation

1. Add the custom repo to Dalamud: `https://raw.githubusercontent.com/tea-time-xiv/pluginmaster/master/pluginmaster.json`
2. Install via the Dalamud plugin installer in XIVLauncher.

## Prerequisites

* XIVLauncher, FINAL FANTASY XIV, and Dalamud must be installed.

## Development

### Building

```bash
dotnet build --configuration Release
```

Output: `Autools/bin/x64/Release/Autools.dll` (when building via the solution; `Autools/bin/Release/` when building the csproj directly).

### Testing in-game

1. Use `/xlsettings` → Experimental, add the full path to the directory containing `Autools.dll` as a Dev Plugin Location.
2. Use `/xlplugins` → Dev Tools → Installed Dev Plugins, enable `Autools`.
3. Run `/passauto` or `/nojog` to toggle features.

## License

AGPL-3.0-or-later. See [LICENSE.md](LICENSE.md) for details.

## Disclaimer

This plugin is 100% AI generated.
