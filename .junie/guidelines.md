### Development Guidelines for Crazy Snake

This document provides project-specific information for developers working on the Crazy Snake project.

#### 1. Build & Configuration

*   **Runtime Environments**: The project uses .NET 8.0 for the main game and .NET 10.0 for the test project. Ensure both SDKs are installed.
*   **Godot Version**: Godot Engine 4.5.1 (Godot.NET.Sdk).
*   **Building**:
    *   Use `dotnet build` from the root directory.
    *   If you encounter "Duplicate attribute" errors (e.g., `AssemblyCompanyAttribute`), run `dotnet clean` before building or testing. This often happens due to Godot's temporary file generation in `.godot/mono/temp/obj/`.
*   **Assets**: Ensure `assets.zip` is extracted into the `assets/` folder in the project root.

#### 2. Testing Information

*   **Framework**: [NUnit 4](https://nunit.org/) is used for unit testing.
*   **Test Project**: Located in the `SnakeTest/` directory.
*   **Running Tests**:
    ```bash
    dotnet test SnakeTest\SnakeTest.csproj
    ```
    *Note: Running `dotnet test` from the root may fail if it attempts to build both projects simultaneously without proper isolation or if there are stale artifacts.*
*   **Adding New Tests**:
    1.  Create a new `.cs` file in the `SnakeTest/` folder.
    2.  Use the `[TestFixture]` and `[Test]` attributes.
    3.  Follow the existing pattern of using `MockObjects.cs` for mocking game components like `ISnakeManager`.
*   **Test Example**:
    ```csharp
    using NUnit.Framework;
    using Godot;

    namespace Snake.Tests;

    [TestFixture]
    public class GuidelineDemoTest
    {
        [Test]
        public void TestVectorAddition()
        {
            var v1 = new Vector2I(1, 2);
            var v2 = new Vector2I(3, 4);
            var result = v1 + v2;
            
            Assert.That(result, Is.EqualTo(new Vector2I(4, 6)));
        }
    }
    ```

#### 3. Additional Development Information

*   **Architecture**:
    *   **Manager-based**: Core logic is orchestrated by `Main.cs`, which delegates to specialized managers (e.g., `SnakeManager`, `ItemManager`, `ScoreManager`).
    *   **Signal-driven**: Communication between managers and the UI is primarily handled via Godot Signals to maintain decoupling.
    *   **Interfaces**: Use interfaces defined in `Scripts/Interfaces/` (e.g., `ISnakeManager`) to facilitate testing and mocking.
*   **Code Style**:
    *   Follow standard C# naming conventions (PascalCase for classes/methods, camelCase for local variables).
    *   Use file-scoped namespaces (C# 10+ feature).
    *   Utilize modern C# features (e.g., pattern matching, switch expressions) where appropriate.
*   **Godot Integration**:
    *   The project uses `Godot.NET.Sdk`. Be aware that some Godot-specific source generators might require the project to be opened in the Godot Editor to correctly resolve paths like `res://`.
    *   Avoid direct logic in `_Process` or `_Input` inside managers; instead, use methods that can be called by `Main.cs` or triggered by signals.
