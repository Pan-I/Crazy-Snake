# Setup and Troubleshooting Tips

This document provides solutions for common issues encountered during setup and development of Crazy Snake.

## Common Issues

### 1. Missing Assets
**Symptoms:** Game starts with missing textures or sounds.
**Solution:** Verify that the `assets/` folder was correctly downloaded with the repository. If it's missing, you may need to re-clone or pull from the main branch. The directory structure should be `assets/sprites/` and `assets/audio/` etc.

### 2. C# Solution Not Found
**Symptoms:** Godot reports errors about missing .sln file or can't build.
**Solution:** In Godot, go to `Project > Tools > C# > Create Solution`.

### 3. Missing .NET SDK
**Symptoms:** Build fails with `dotnet` command not found.
**Solution:** Install .NET 8.0 or 10.0 SDK from [Microsoft's website](https://dotnet.microsoft.com/download).

### 4. IDE Integration Issues
**Symptoms:** Rider or Visual Studio doesn't show Godot-specific classes.
**Solution:** Ensure you have the Godot support plugin installed in your IDE.

### 5. Reloading Scripts & PackedScene Link
**Symptoms:** PackedScene objects are not instanciated.

<figure class="center medium">
  <img src="Errors\PackedScene_Load\PackedScene_Error.png" alt="Null reference debug error">
  <figcaption>Null reference debug error</figcaption>
</figure>

**Solution:** in Godot, load the relevant `.tscn` file to the script.
**Possible Causes:** Script was re-attached to node in Godot; common with `Main.cs` and `Snake Segment Ps`

<figure class="center medium">
  <img src="Errors\PackedScene_Load\Packed_Scene_Cause.png" alt="Godot exported variable load cause and fix">
  <figcaption>Godot exported variable load cause and fix</figcaption>
</figure>

<figure class="center medium">
  <img src="Errors\PackedScene_Load\PackedScene_Solution.png" alt="Scene select">
  <figcaption>Scene select</figcaption>
</figure>

### 6. Warnings: .uid:
**Symptoms:** Debug window fills with warnings of .uid errors and using literal paths instead.

**Solution:** Ideal: run `Scripts\fix.gd` from Godot engine/terminal; Alternate: Re-save all scenes prompting the warning.

[**More Info (Godot Forum)**](https://forum.godotengine.org/t/since-export-loading-project-gives-invalid-uid-warnings/97325)

---
*Add more tips here as they are discovered.*
