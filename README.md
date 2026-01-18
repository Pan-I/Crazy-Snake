#  🐍🎮 Crazy Snake 👾🪩 - v0.3.2.0

<img src="https://raw.githubusercontent.com/Pan-I/Crazy-Snake/refs/heads/main/readme_links/Banner%20Image.png" alt="game banner" width="900"/>

## 📖 Table of Contents
- [Introduction & Updates](#-introduction--update-news-)
- [AI Agent Instructions](readme_links/AGENTS.md)
- [Architecture](readme_links/ARCHITECTURE.md)
- [Features](#-features-)
- [Stack & Tech](#-stack--tech-)
- [Project Structure](#-project-structure-)
- [Getting Started](#-getting-started-)
- [Setup & Troubleshooting](readme_links/SETUP_ISSUES.md)
- [Requirements/Environment](#-requirements-environment-)
- [Scripts & Development](#-scripts--development-)
- [Testing](#-testing-)
- [AI Agent Instructions](readme_links/AGENTS.md)
- [Issues, Enhancements & Progress](#-issues-enhancements--progress-)
- [Contributing](#-contributing-)
- [Attributions](#-attributions-)
- [License](#-license-)

---
## 🎉 Introduction & Update News 📣

A remake of Snake, with some twists, as a Godot project. Written entirely in C#. Game includes a long list of items that have a wide range of effects on the game and game play, as well as a complex and high-scoring point system.

#### Current Major Update Set (v0.3.2)

|                                                               *Where v0.3.x is now*                                                               |
| :-----------------------------------------------------------------------------------------------------------------------------------------------: |
| <img src="https://github.com/Pan-I/Crazy-Snake/blob/main/readme_links/Gameplay%20Preview/v0.3.1%20Gameplay.gif" alt="gameplay gif" width="600"/> |
|                                                               *Where v0.3.x is now*                                                               |

The current version (v0.3.x) focuses on:
- GUI overhaul
- Health System
- Combo scoring system
- Ouroboros mechanics
- Speed changes
- Architecture Refactoring
- Documentation

|                                                                    *Where v0.3. started*                                                                     |
| :----------------------------------------------------------------------------------------------------------------------------------------------------------: |
| <img src="https://github.com/Pan-I/Crazy-Snake/blob/main/readme_links/Progress/v0.2.3.0%20Gameplay.gif?raw=true" alt="broken link of game screenshot 3" width="450"/> |
|                                                                    *Where v0.3. started*                                                                     |
---
## 🏗 Architecture
Detailed architectural overview can be found in [ARCHITECTURE.md](readme_links/ARCHITECTURE.md).

The project follows a **Manager-based architecture** orchestrated by `Main.cs`. Communication is primarily handled via **Godot Signals**.

---
## 🌟 Features 🎁
- **Godot 4 Core**: Built with the latest Godot Engine features.
- **Pure C#**: Leverages C# 12 and .NET 8.
- **Complex Scoring**: Multiplier-based combo system.
- **Dynamic Gameplay**: Variety of items with unique effects (speed, health, control reversal).
- **Unit Tests**: Includes a test suite for core logic.

---
## 💻 Stack & Tech 🛠
- **Language**: C# 12
- **Framework**: Godot Engine 4.x (Godot.NET.Sdk)
- **Runtime**: .NET 8.0 / .NET 10.0
- **Package Manager**: NuGet (integrated via Godot/.NET)

---
## 📂 Project Structure
```plain text
.
├── assets/             # Game sprites, audio, and resources (needs unzipping)
├── scenes/             # Godot Scene files (.tscn)
├── Scripts/            # C# Source Code
│   ├── Domain/
│   │   ├── Managers/   # Game logic orchestrators (Snake, Score, Items, etc.)
│   │   └── Utilities/  # Pure logic helpers
│   ├── GUI Scripts/    # UI-specific behavior
│   ├── Interfaces/     # Decoupling abstractions
│   └── Main.cs         # Main Entry Point / Game Controller
├── SnakeTest/          # NUnit Test Project
├── project.godot       # Godot Project File
└── Snake.sln           # C# Solution
```

---
## 🏃 Getting Started 🚀
1. **Clone the repository**:
   ```bash
   git clone https://github.com/Pan-I/Crazy-Snake.git
   ```
2. **Extract Assets**:
   Unzip `assets.zip` into the project root so that an `assets/` folder exists.
3. **Open in Godot**:
   - Open Godot Engine 4.x.
   - Import the project by selecting `project.godot`.
4. **Generate Solution**:
   In Godot: `Project > Tools > C# > Create Solution` (if not already present).
5. **Build & Run**:
   Press `F5` in Godot or use your preferred IDE.

For common setup problems, check [SETUP_ISSUES.md](readme_links/SETUP_ISSUES.md).

---
## 🔨 Code Highlights 🔍
- **Orchestration**: High-level gameplay beats are handled in [`Main.cs`](Scripts/Main.cs)
- **Item Management**: Item rarity, spawning, and rules are handled in [`ItemManager.cs`](Scripts/Domain/Managers/ItemManager.cs)
- **Snake Logic**: Movement, growth, and animations are handled in [`SnakeManager.cs`](Scripts/Domain/Managers/SnakeManager.cs)
- **Scoring**: Combo logic is encapsulated in [`ScoreManager.cs`](Scripts/Domain/Managers/ScoreManager.cs)

---
## 💻 Requirements/Environment 💽
- [Godot Engine 4.x (.NET Edition)](https://godotengine.org/)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Recommended) [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio](https://visualstudio.microsoft.com/) with Godot support.

---
## 📜 Scripts & Development
- **Build**: Use `dotnet build` in the root or build via Godot.
- **Environment Variables**: None required for standard run. TODO: Add any CI/CD specific variables if implemented.

---
## 🧪 Testing 🏁
The project uses **NUnit** for unit testing core domain logic.
- **Run Tests via CLI**:
  ```bash
  dotnet test
  ```
- **Test Location**: `SnakeTest/`

---
## 🐛 Issues & Troubleshooting
- See [known issues](https://github.com/Pan-I/Crazy-Snake/issues).
- For common setup problems, check [SETUP_ISSUES.md](readme_links/SETUP_ISSUES.md).

---
## 🔧 Contributing 💾
Contributing information found here: [CONTRIBUTING.md](readme_links/CONTRIBUTING.md)

---
## 📍 Attributions 👥
- Inspired by: [Beginner Godot Tutorial - Make a Snake Game in 10 Minutes!](https://www.youtube.com/watch?v=DlRP-UBR-2A)
- Audio credits: [**ATTRIBUTIONS.md**](readme_links/ATTRIBUTIONS.md)

---
## 📜 License 🔓
Crazy Snake is licensed under **GPL-2.0**.
- **Sprites**: [CC-BY-4.0](https://creativecommons.org/licenses/by/4.0/) Ian Pommer.
- **Audio**: See [ATTRIBUTIONS.md](readme_links/ATTRIBUTIONS.md).
- **Godot Engine**: [MIT License](https://github.com/godotengine/godot/blob/master/LICENSE.txt).
