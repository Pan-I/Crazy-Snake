# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.3.1] - (In Development)
### Added
- Change log; this file!
- Basic unit tests for some gameplay logic.

### Changed
- HUD now flashes on and off (with sfx) with low-health status.
- Switching of player controls by eating rotten egg has been restored as a rule. 

### Fixed
 - Speed update bug.
 - Animation popping during combo has been reduced; still happening to a lesser degree.

## [0.3.3] - 2026-01-25
### Added
- Audio Implementation: Background music and sound effects integrated across game states. [#49]
- Comprehensive XML documentation was added to all manager classes for better IntelliSense support.
- Architecture visualization: Mermaid chart added to `ARCHITECTURE.md`.

### Changed
- Domain Refactor: Managers updated for consistency and better separation of concerns.
- Documentation cleanup and standardization.

### Fixed
- Miscellaneous GUI bug fixes and layout adjustments.

## [0.3.2] - 2026-01-17
### Added
- Major Architectural Refactor: Implementation of domain-specific managers (`SnakeManager`, `ItemManager`, `ScoreManager`, `TimeManager`, `UIManager`). [#48]
- Signal-driven communication: Decoupled managers from the UI and each other using Godot signals.
- Namespace standardization across the entire project.

### Changed
- Improved resource cleanup and memory management.
- Updated README and internal documentation for the new architecture.

### Fixed
- Resolved various edge-case bugs in item placement and UI synchronization.

## [0.3.1] - 2025-04-24
### Added
- Health System: Support for multiple lives and damage mechanics. [#46]
- Combo System: Score multipliers for consecutive item collections.
- Self-eating mechanic: Snake now takes damage when colliding with its own body.
- Enhanced GUI Feedback: HUD flashing effects and a dynamic health bar. [#42]

### Changed
- Scoring Rebalance: Adjusted point values and combo thresholds.
- Movement Speed: Refined speed increments for a more progressive challenge.

### Fixed
- Egg-Death bug: Fixed a critical issue where certain item interactions caused immediate game over. [#30]

## [0.3] - 2025-04-09
### Added
- GUI Overhaul Skeleton: New layout with dedicated placeholders for lives, combos, and status. [#29]
- Quit functionality: Added 'Esc' key support to exit the game.

### Changed
- GUI Dressing: Updated UI assets and theme for a more polished look.

### Fixed
- Border collision bug fixes: Improved reliability of snake-to-wall collision detection. [#28]

## [0.2.3.1] - 2025-02-08
### Changed
- General code refactoring and project-wide cleanup. [#27]
- Updated gameplay preview assets and documentation links.

### Fixed
- Miscellaneous bug fixes identified during testing. [#26]

## [0.2.3] - 2025-02-06
### Added
- Death Animations: New visual sequences and specialized sprites for the snake's demise. [#25] [#23]
- Robust Spawn Checks: Implemented wall-trap and full-board checks to prevent impossible item spawns.

### Changed
- Polished Pause: Improved the responsiveness and visual state of the pause menu. [#24]

### Fixed
- Fixed missing effect for the Frog item.

## [0.2.2.3] - 2025-01-18
### Changed
- Item Spawning Refactor: Significant improvements to the logic determining where and when items appear. [#22]

### Fixed
- Resolved multiple item spawn issues, including overlaps and out-of-bounds spawning. [#21]

## [0.2.2.2] - 2025-01-17
### Added
- Tail Polish: Improved animations and transitions for the winding tail tip. [#20]

### Changed
- Body-bend Refactor: Cleaned up the logic and scenes for snake body segments.
- Improved Pause logic: Better handling of input during paused states. [#17] [#16]
- Point Balancing: Adjusted point values for Skull and other penalty items.

## [0.2.2] - 2025-01-12
### Added
- Animated Snake Sprites: Distinct animations for Head, Bending Body, and Winding Tail. [#15]
- Tail Growth & Rotation: Implemented complex tail segments that grow and turn realistically. [#14] [#12]

### Changed
- Visuals: Background updated to use `AnimatedSprite2D`.

## [0.2] (Unsigned/Unofficial) - 2025-01-8
### Added
- Dynamic Item Generation system with various item types. [#2]

### Changed
- Asset Management: Switched to `ResourcePreloader` for more efficient loading.

## [0.1] (Unsigned/Unofficial) - 2025-01-05
### Added
- Base snake game logic with movement, egg collection, snake growth, scoring, and end game conditions.

## [0.0.1.2] - 2024-12-14
### Added
- Initial project structure and Godot configuration.
- Basic snake movement and item collection stubs.
- Core repository documentation (README, License, etc.).

[#49]: https://github.com/Pan-I/Crazy-Snake/pull/49
[#48]: https://github.com/Pan-I/Crazy-Snake/pull/48
[#46]: https://github.com/Pan-I/Crazy-Snake/pull/46
[#42]: https://github.com/Pan-I/Crazy-Snake/pull/42
[#30]: https://github.com/Pan-I/Crazy-Snake/pull/30
[#29]: https://github.com/Pan-I/Crazy-Snake/pull/29
[#28]: https://github.com/Pan-I/Crazy-Snake/pull/28
[#27]: https://github.com/Pan-I/Crazy-Snake/pull/27
[#26]: https://github.com/Pan-I/Crazy-Snake/pull/26
[#25]: https://github.com/Pan-I/Crazy-Snake/pull/25
[#24]: https://github.com/Pan-I/Crazy-Snake/pull/24
[#23]: https://github.com/Pan-I/Crazy-Snake/pull/23
[#22]: https://github.com/Pan-I/Crazy-Snake/pull/22
[#21]: https://github.com/Pan-I/Crazy-Snake/pull/21
[#20]: https://github.com/Pan-I/Crazy-Snake/pull/20
[#17]: https://github.com/Pan-I/Crazy-Snake/pull/17
[#16]: https://github.com/Pan-I/Crazy-Snake/pull/16
[#15]: https://github.com/Pan-I/Crazy-Snake/pull/15
[#14]: https://github.com/Pan-I/Crazy-Snake/pull/14
[#12]: https://github.com/Pan-I/Crazy-Snake/pull/12
[#2]: https://github.com/Pan-I/Crazy-Snake/pull/2


