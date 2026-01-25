# Architecture: Crazy Snake

This document outlines the high-level architecture of **Crazy Snake**, a twist on the traditional Snake game built with Godot 4 and C#.

## Overview

The project follows a **Manager-based architecture** where a central controller (`Main.cs`) orchestrates specialized domain managers. Communication between components is primarily handled via **Godot Signals** to maintain loose coupling while allowing for complex interactions (like item effects modifying score or health).

## Core Components

### 1. Orchestration (`Main.cs`)
The `Main` class serves as the entry point and the "brain" of the game.
- **Responsibilities:**
    - Initializing all managers and injecting dependencies.
    - Connecting signals between managers (e.g., connecting `SnakeManager` movement to `HealthManager` logic).
    - Managing the primary game loop state (`NewGame`, `StartGame`, `EndGame`).
    - Handling high-level gameplay checks like `CheckEggEaten`, `CheckSelfEaten`, and `CheckItemHit`.

Future separation between object instantiation and signal connections, and  gameplay logic, may happen betwen `Main.cs` and the current stub, `GameController.cs`, respectively.

### 2. Domain Managers (`Scripts/Domain/Managers/`)
The business logic is divided into specialized managers, most of which inherit from `GodotObject` to leverage signals.

- **SnakeManager**: Manages the snake's physical presence, segment data, and movement logic. It handles the complex visual bending of the snake's neck and tail.
- **ItemManager**: Handles the spawning and lifecycle of items (eggs, walls, power-ups). It contains the placement logic to ensure items don't spawn inside the snake or in unreachable "traps."
- **ScoreManager**: Tracks the current score and manages the **Combo System**. It calculates multipliers (Combo X and Y) and processes the transition between regular play and "Crazy Combo" mode.
- **HealthManager**: Tracks player lives and manages the visual "Health Segments" (represented as snake segments) on the HUD.
- **UiManager**: Encapsulates all interactions with the Godot CanvasLayers. It handles score updates, HUD flashing effects, and game-over screens.
- **BoardManager**: Defines the grid boundaries and provides utility methods to check for out-of-bounds conditions.
- **TimeManager**: Wraps Godot `Timer` nodes to control game speed and effect durations.

### 3. Utilities (`Scripts/Domain/Utilities/`)
- **ItemEffectLogic**: A specialized class used by the `ItemManager` to apply gameplay modifiers (like reversing controls or restoring them) when specific items are consumed.
- **InputLogic**: Handles input mapping and transformations.

## Data Flow & Communication

### Signal-Driven Updates
Instead of managers calling each other directly, they emit signals that `Main.cs` listens for. 
*Example:* 
1. `ItemManager` detects a "Rotten Egg" was hit.
2. It emits `ScoreChanged`.
3. `Main` receives this and calls `Score.AddScore(...)` and `UI.HudFlash(...)`.
4. Some methods will emit further signals when other managers need to act next, like changing the GUI when the score is changed.

```mermaid
---
config:
  theme: redux
  layout: dagre
---
flowchart TB
 subgraph s1["Main Scene"]
        A("Main Script")
        n4["Event Handler Method"]
        s2["s2"]
        n3["Scene Signal"]
        B1["New Game Method"]
  end
 subgraph s2["Game Over Menu Scene"]
        n1["New Game Button Press"]
        n2["Scene Signal"]
        J["Game Over Script"]
        n5["Event Handler Method"]
  end
    n1 --> n2
    n2 --> J
    J --> n5
    n5 --> n3
    n3 --> A
    B1 --> C["Board Manager Script"] & D["Snake Manager Script"] & E["Item Manager Script"] & F["Score Manager Script"] & G["Time Manager Script"] & H["Health Manager Script"]
    B1 L_B1_I_0@==> I["UI Manager Script"]
    C --> C1["Clear Board Method"]
    D --> D1["Reset Snake Method"]
    E --> E1["Clear Item Method"]
    F --> F1["Reset Score Method"]
    G --> G1["Reset Timers Method"]
    H --> H1["Reset Health Method"]
    I L_I_I1_0@==> I1["Reset GUI Method"]
    F1 L_F1_n6_0@==> n6["Scene Signal"]
    n6 L_n6_A_0@==> A
    n4 --> B1
    H1 L_H1_n6_0@==> n6
    A --> n4
    A L_A_n4_2@==> n4
    n4 L_n4_B1_2@==> B1

    n4@{ shape: subproc}
    n3@{ shape: com-link}
    B1@{ shape: subproc}
    n1@{ shape: display}
    n2@{ shape: com-link}
    n5@{ shape: subproc}
    C1@{ shape: subproc}
    D1@{ shape: subproc}
    E1@{ shape: subproc}
    F1@{ shape: subproc}
    G1@{ shape: subproc}
    H1@{ shape: subproc}
    I1@{ shape: subproc}
    n6@{ shape: com-link}
    style I fill:#C8E6C9,color:#000000
    style F1 fill:#00C853
    style H1 fill:#00C853
    style I1 fill:#C8E6C9,color:#000000
    style n6 fill:#00C853
    linkStyle 11 stroke:#00C853,fill:none
    linkStyle 18 stroke:#00C853,fill:none
    linkStyle 19 stroke:#00C853,fill:none
    linkStyle 20 stroke:#00C853,fill:none
    linkStyle 22 stroke:#00C853,fill:none
    linkStyle 24 stroke:#00C853,fill:none
    linkStyle 25 stroke:#00C853,fill:none

    L_B1_I_0@{ animation: slow } 
    L_I_I1_0@{ animation: slow } 
    L_F1_n6_0@{ animation: slow } 
    L_n6_A_0@{ animation: slow } 
    L_H1_n6_0@{ animation: slow } 
    L_A_n4_2@{ animation: slow } 
    L_n4_B1_2@{ animation: slow }
```

### The Combo System
The game features a unique scoring mechanic:
- **Tally**: Every egg eaten increases a tally.
- **Activation**: At a tally of 7, `ScoreManager` enters `IsInCombo` mode.
- **Calculation**: During a combo, score is calculated as `ComboPointsX * ComboPointsY`. Different items modify these variables differently (e.g., `Mushroom` starts/modifies X, while `Frog` modifies Y).

## Project Structure

```plain text
../Scripts/
├── Domain/
│   ├── Managers/     # Business logic (Snake, Score, Items, etc.)
│   └── Utilities/    # Pure logic helpers (Input, Item Effects)
├── GUI Scripts/      # Specific UI event handlers (Menus)
├── Interfaces/       # Decoupling contracts (e.g., ISnakeManager)
├── ../Main.cs        # Central Game Controller/Orchestrator
└── ../GameController.cs # (Under Construction)
```


## Key Technologies
- **Engine**: Godot 4.x
- **Language**: C# 12 (.NET 8)
- **Patterns**: Observer (Signals), Dependency Injection (via `Main.cs` initialization), and Component-based UI.

## Future Considerations
- **AudioManager**: Currently under construction; will eventually be integrated via the `Main` class to respond to gameplay signals.
- **Power-up Logic**: The `ItemEffectLogic` is designed to be expandable for more complex "twists" beyond control reversal.