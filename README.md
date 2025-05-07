#  🐍🎮 Crazy Snake 👾🪩 - v0.3.1.0


<img src="https://raw.githubusercontent.com/Pan-I/Crazy-Snake/refs/heads/main/readme_links/Banner%20Image.png" alt="broken link of game screenshot 1" width="900"/>


##### ReadMe Table of Contents
- [Introduction & Updates](https://github.com/Pan-I/Crazy-Snake?tab=readme-ov-file#-introduction-)
- [Features](https://github.com/Pan-I/Crazy-Snake?tab=readme-ov-file#-features-)
- [Code Highlights](https://github.com/Pan-I/Crazy-Snake?tab=readme-ov-file#-code-highlights-)
- [Getting Started](https://github.com/Pan-I/Crazy-Snake?tab=readme-ov-file#-getting-started-)
- [Requirements/Environment](https://github.com/Pan-I/Crazy-Snake?tab=readme-ov-file#-requirementsenvironment-)
- [Issues, Enhancements & Progress](https://github.com/Pan-I/Crazy-Snake?tab=readme-ov-file#-issues-enhancements--progress-)
- [Contributing](https://github.com/Pan-I/Crazy-Snake?tab=readme-ov-file#-contributing-)
- [Attributions](https://github.com/Pan-I/Crazy-Snake?tab=readme-ov-file#-attributions-)
- [License](https://github.com/Pan-I/Crazy-Snake?tab=readme-ov-file#-license-)

---
## 🎉 Introduction & Update News 📣

A remake of Snake, with some twists, as a Godot project. Written entirely in C#. Game includes a long list of items that have a wide range of effects on the game and game play, as well as a complex and high-scoring point system.

#### Current Major Update Set (v0.3.1)

|                                                                   *Where v0.3.x is now*                                                                    |
| :--------------------------------------------------------------------------------------------------------------------------------------------------------: |
| <img src="https://github.com/Pan-I/Crazy-Snake/blob/main/readme_links/v0.3.1%20Gameplay.gif?raw=true" alt="broken link of game screenshot 2" width="600"/> |
|                                                                   *Where v0.3.x is now*                                                                    |


The current version (v0.3.x) will focus on the following, with more details to come:
- GUI overhaul
	- Shiny new look to give game a bit more of an atmosphere
	- Flashing HUD when interacting with items, or low on health
- Health System
	- Certain items deduct
	- Certain items like the large wall, and edges of board still end game
- Combo scoring system
	- Triggered by eating 7 regular eggs without hitting a wall or eating a bad egg
	- X * Y scoring system with different items impacting different parts of the score in various ways
	- Can be jump started with certain items
	- Can be ended by hitting walls
	- Can be canceled entirely by certain items
- Ouroboros - snake can now bite its own tail, at the deduction of health
- Speed changes
	- As snake grows, it will gain a little bit a speed - with a limit
	- Combo-state speeds up snake
	- Leaving combo-state slows down snake

|                                                                    *Where v0.3. started*                                                                     |
| :----------------------------------------------------------------------------------------------------------------------------------------------------------: |
| <img src="https://github.com/Pan-I/Crazy-Snake/blob/main/readme_links/v0.2.3.0%20Gameplay.gif?raw=true" alt="broken link of game screenshot 3" width="450"/> |
|                                                                    *Where v0.3. started*                                                                     |


---
## 🌟 Features 🎁
#### Full components needed to build the project, including:
- Godot project file
- Godot Scenes 
- Scripts (.cs files) complete with in-code comments and summaries
- Zipped folder of sprites and audio files

---
## 🔨 Code Highlights 🔍

- High-level game play beats are handled in [`Main.cs`](https://github.com/Pan-I/Crazy-Snake/blob/main/scripts/main.cs)
- Item rarity, spawning, and rules are handled in [`Items.cs`](https://github.com/Pan-I/Crazy-Snake/blob/main/scripts/Items.cs)
- Snake movement, growth, and animations are handled in [`Snake.cs`](https://github.com/Pan-I/Crazy-Snake/blob/main/scripts/Snake.cs)

---
## 🏃 Getting Started 🚀
0. Requirements/Prerequisites (*below*)
1. Clone project
2. Unzip assets folder.
3. Open Godot Engine
4. Select import and navigation to cloned project location
5. Open the `project.godot` file
6. Build a `.sln` file, preferably directly from Godot 
	`Project > Tools > C# > Create Solution`
7. Build project for the first time
8. For exporting configuration [read here](https://docs.godotengine.org/en/stable/tutorials/export/index.html)

---
## 💻 Requirements/Prerequisites 💽
You will need to install the Godot game engine to build, run and export the game.
- [Godot Engine](https://godotengine.org/)
- [Godot Documentation](https://docs.godotengine.org/en/stable/index.html)

If you would like to debug the game, you will need a C# IDE that is compatible with Godot. You will need to configure your IDE, and change some Godot settings before debugging from IDE.
- C#, Godot-compatible IDE
	- [Rider](https://www.jetbrains.com/rider/) 
	- [Visual Studio](https://visualstudio.microsoft.com/) 
- Configuration & plugins:
	- [Godot IDE Config](https://docs.godotengine.org/en/stable/contributing/development/configuring_an_ide/index.html)
	- [Rider](https://plugins.jetbrains.com/plugin/13882-godot-support)
	- Visual Studio configuration unknown

---
## 🐛 Issues, Enhancements, & Progress 🔮

There is a list of [known issues.](https://github.com/Pan-I/Crazy-Snake/issues) If you have suggestions, requests, or come across any problems, you can submit a [new issue ticket here.](https://github.com/Pan-I/Crazy-Snake/issues/new/choose) You can find the [progress of submitted tickets here.](https://github.com/users/Pan-I/projects/6)

---
## 🔧 Contributing 💾

Contributing information found here: [CONTRIBUTING.md](https://github.com/Pan-I/Crazy-Snake/blob/main/.github/CONTRIBUTING.md)

---
## 📍Attributions 👥

The project is initially inspired by, and was made v0.1 by the following video: [Beginner Godot Tutorial - Make a Snake Game in 10 Minutes!](https://www.youtube.com/watch?v=DlRP-UBR-2A)

The credits for the audio used, are in [**ATTRIBUTIONS.md**](https://github.com/Pan-I/Crazy-Snake/tree/main/readme_links/ATTRIBUTIONS.md)

This project was heavily inspired by the following videos:

---
## 📜 License 🔓

Crazy Snake, a remake of 'Snake'. A Godot project written in C#.
Copyright GPL-2.0 (c) (2025) Ian Pommer

This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version. [Click here for details](https://github.com/Pan-I/Crazy-Snake?tab=GPL-2.0-1-ov-file#readme)

##### Sprites
All sprites (.png files) are the products created solely by the repo owner (Pan-I, Ian Pommer). If separated from the Crazy Game product as a whole, the sprites are under [CC-BY-4.0 (2024-2025) Ian Pommer.](https://creativecommons.org/licenses/by/4.0/legalcode.en) Please give proper credit if copied and redistributed, or remix, transform, and built upon. You must give [appropriate credit](https://creativecommons.org/licenses/by/4.0/#ref-appropriate-credit) , provide a link to the license, and [indicate if changes were made](https://creativecommons.org/licenses/by/4.0/#ref-indicate-changes) . You may do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use.

##### Audio
All audio files are under various creative commons or attribution licenses, further details and credits can be found here: [**ATTRIBUTIONS.md**](https://github.com/Pan-I/Crazy-Snake/tree/main/readme_links/ATTRIBUTIONS.md)

### Godot Game Engine:
Author: [Juan Linietsky, Ariel Manzur, and contributors](https://godotengine.org/contact)  
Source: [godotengine.org](https://godotengine.org/)  
License: [MIT License](https://github.com/godotengine/godot/blob/master/LICENSE.txt)

---
