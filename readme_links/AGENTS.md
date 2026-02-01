# AI Agent Instructions for Crazy Snake

Welcome, AI Agent! This document is designed to help you navigate and understand the **Crazy Snake** project efficiently.

## 🚀 Quick Start
- **Primary Tech Stack**: Godot 4.5.1 (Godot.NET.Sdk), C# 12, .NET 8.0 (Game), .NET 10.0 (Tests).
- **Solution File**: `Snake.sln`
- **Project Root**: `snake/` (contains `project.godot`)
- **Test Root**: `snake_test/`

## 📂 Project Structure & Key Files
- `snake/project.godot`: The Godot project configuration. Defines the main scene (`res://scenes/main.tscn`) and input mappings.
- `snake/scripts/Main.cs`: The central orchestrator/brain of the game. Handles initialization and signal connections between managers.
- `snake/scripts/domain/managers/`: Contains specialized managers (Snake, Item, Score, Health, UI, Board, Time, Audio).
- `snake/scripts/interfaces/`: Interface definitions for managers to facilitate testing/mocking.
- `snake_test/`: NUnit 4 test project.

## 🏗 Architectural Patterns
- **Manager-based Architecture**: `Main.cs` orchestrates specialized managers. Avoid putting heavy logic in `Main.cs`; delegate to managers.
- **Signal-driven Communication**: Managers emit Godot signals; `Main.cs` (or other managers) connects to them. This maintains loose coupling.
    - *Example*: `Items.ItemSpawned += (node) => AddChild(node);` in `Main.cs`.
- **Immutability & Setters**: Use setter methods in Managers (e.g., `SetCellPixelSizeRef`) to enforce state control rather than direct property modification where possible.
- **Scene-Script Decoupling**: Scripts are generally detached from Godot `_Process` or `_Input` loops where possible, instead using methods called by `Main.cs` or signals.

## 🔗 Godot res:// Path Mapping
When you see `res://` in the code, it maps to the `snake/` directory:
- `res://scenes/` -> `snake/scenes/`
- `res://scripts/` -> `snake/scripts/`
- `res://assets/` -> `snake/assets/`
- `res://project.godot` -> `snake/project.godot`

## 🛠 Development Workflow
- **Building**: Run `dotnet build` from the root. If you see "Duplicate attribute" errors, run `dotnet clean` first.
- **Testing**: Run tests using:
  ```bash
  dotnet test snake_test\snake_test.csproj
  ```
- **Adding Logic**: 
  1. Define interfaces in `scripts/interfaces/`.
  2. Implement in `scripts/domain/managers/`.
  3. Wire up in `Main.cs`.

## 🤖 Navigation Tips for Agents
- **Symbols**: Use your search tools to find "Manager" to see all domain orchestrators.
- **Signals**: Search for `[Signal]` or `delegate void ...EventHandler` to understand the event flow.
- **Godot Nodes**: Check `Main.cs`'s `InitializeManagers` method to see how Godot nodes are mapped to C# manager properties.
- **Inputs**: Refer to `project.godot` under the `[input]` section for mapped actions (e.g., `move_up`, `pause`).

## 📜 Coding Standards
- **Naming**: PascalCase for classes/methods, camelCase for local variables.
- **Namespaces**: Use file-scoped namespaces (e.g., `namespace Snake.Scripts;`).
- **Main.cs**: Always maintain capitalization for `Main.cs` references.
- **Comments**: Follow the existing XML documentation style for public members.
