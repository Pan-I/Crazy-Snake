### 🤖 AI Agent Instructions

This document provides instructions and context for AI agents working on the Crazy Snake project. These guidelines are model-agnostic and intended to help any AI assistant understand the project's standards and workflows.

#### 📋 Core Guidelines
For detailed development guidelines, including build instructions, testing procedures, and architectural overview, please refer to the primary guidelines:

👉 **[Development Guidelines](../.junie/guidelines.md)**

#### 🛠 AI Agent Expectations
When performing tasks in this repository, agents should:

1.  **Strictly Follow Guidelines**: Adhere to the patterns defined in `guidelines.md`.
2.  **Maintain Consistency**: Match the existing code style, naming conventions, and architectural patterns (Manager-based, Signal-driven).
3.  **Test-Driven Development**: Always consider existing tests in `snake_test/` and add new tests for any core logic changes as prescribed in the guidelines.
4.  **Godot Integration**: Be mindful of Godot-specific requirements, such as the `Godot.NET.Sdk` and the need for occasional `dotnet clean` to resolve source generator issues.
5.  **Documentation**: Keep READMEs and other documentation up to date when making significant changes.

#### 🔗 Quick References
- **Main Entry Point**: `scripts/Main.cs`
- **Domain Logic**: `scripts/domain/`
- **Tests**: `snake_test/`
- **Project Configuration**: `project.godot`
