# CoreShift.Unity — Unity Adapter

These scripts are the **Unity-facing adapter**. They are intentionally **not part of the .NET
solution** and are **not compiled in this repository** (Unity is not available here). They exist so
the Unity team can drop them into a Unity 2D project that references the compiled
`CoreShift.Core.dll` and `CoreShift.Data.dll`.

## Setup

1. Build the core: `dotnet build src/CoreShift.Core/CoreShift.Core.csproj -c Release` (and Data).
2. Copy `CoreShift.Core.dll` and `CoreShift.Data.dll` into `Assets/Plugins/`.
3. Add these scripts under `Assets/Scripts/`.
4. Create an empty GameObject named `CoreShiftRunner` and attach `CoreShiftRunner.cs`.

## Responsibility split

- **Core (`CoreShift.Core`)**: simulation — movement, combat, waves, upgrades, saves. No Unity.
- **Adapter (these scripts)**: read Unity input into `InputState`, call `world.Tick()`, and render
  entity snapshots with pooled GameObjects. The adapter never implements game rules.

The simulation runs at a fixed 60 Hz step. The adapter accumulates `Time.deltaTime` and calls
`world.Tick()` zero or more times per frame so gameplay is frame-rate independent.
