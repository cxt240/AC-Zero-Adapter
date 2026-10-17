# AC Zero Adapter

Keyboard, mouse, and display settings for the Ace Combat Zero preorder port that installs inside Ace Combat 8. The port already has some control bindings, but it will not take most of the changes you would want. This program writes the flight binds in `ACZero\settings\dinput.ini` and the screen mode in `setting.ini`.

Unofficial, and not affiliated with Bandai Namco. You need your own install of the game.

Built with AI assistance, for PC players using a keyboard and mouse instead of a joystick.

## Download

Download `ACZeroAdapter.exe` from the [latest release](https://github.com/cxt240/AC-Zero-Adapter/releases/latest). It is a Windows program and already includes the .NET runtime, so there is nothing else to install. The exe is not stored in the git repository. GitHub rejects a file this large.

Close Ace Combat Zero, run the exe, and press Apply. The game folder box starts at a common Steam location. Use Browse if Zero is somewhere else. Selecting the Ace Combat 8 folder also works; the program looks for `ACZero` inside it.

## Build

Building is only for changing the program. Install the [.NET 9 SDK](https://dotnet.microsoft.com/download), then from this folder:

```powershell
dotnet test
dotnet publish src/ACZeroAdapter/ACZeroAdapter.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugSymbols=false -o publish
```

The program is `publish\ACZeroAdapter.exe`.

Pushing a version tag such as `v1.0.0` runs the release workflow, which tests the project and attaches a new exe to that GitHub release.

Quit the game completely after Apply. A mission that is already running keeps the old controls. If the game is open, Apply asks before it writes.

## Window

**Flight** is the stick and the mouse: throttle, roll, yaw, which axis the mouse drives, and sensitivity. **Camera and weapons** is the look keys, view, boresight, radar, gun, missile, weapon, target, and autopilot.

**Display** is Borderless or Windowed, plus resolution. The port has no exclusive fullscreen. Borderless is the fullscreen mode, and it fills the desktop.

Click a key button and press a key. Escape cancels that click. A key already used by one control is left unset on the other. Escape and Backspace stay bound to pause.

## Controls

| Input | Action |
| --- | --- |
| W, Space | Throttle up |
| S, Left Ctrl | Throttle down |
| A / D | Roll left / right |
| Mouse up / down | Pitch. Mouse up raises the nose when that box is checked |
| Mouse left / right | Yaw |
| Q / E | Yaw |
| Z or C | Autopilot |
| Left Alt + mouse | Camera |
| Left mouse / right mouse | Gun / missile |
| Mouse wheel | Switch weapon |
| Middle mouse, T | Switch target |
| R | Boresight |
| V | Change view. Cycles third person and cockpit |
| M | Radar |
| Esc, Backspace | Pause |

Yaw in Zero is the rudder. Turning the jet is still roll, then pitch. If horizontal mouse movement feels too weak, set Mouse left/right to Roll.

The sensitivity numbers are how far the mouse must move for full deflection. Smaller means more sensitive. Pitch starts at `2`. Horizontal starts at `8`.

## Fullscreen

The launcher reads `setting.ini`. If its screen mode is still Windowed when you press play, it writes `Windowed` back. Leave the launcher on borderless, or set `SkipLauncher=True` in `setting.ini` after the picture looks right.

## Restore

**Restore original** puts the port's keyboard layout back and sets the screen mode to Windowed. The resolution stays as it is. The first time the adapter sees the game, it copies `dinputSample.ini` to `dinput.original.ini` in the game's `settings` folder and restores from that copy. The stock layout is not kept in this folder. The first Apply also saves `dinput.ini.before-adapter` and `setting.ini.before-adapter` beside the game files.
