# Changelog - Rhythm Platformer (working title)

## Format

## ## [0.0.X] - 2022-xx-xx
## ### Added
## - Changes
## 
## ### Changed
## - Changes
## 
## ### Removed
## - Changes
## 
## ### Fixed
## - Changes
## 
## ### Deprecated
## - Soon to be changed

## [Unreleased] (terminal: git log; hold enter to go farther back in commit history)

## [0.0.15] - 2022-10-27
### Added
- Main Menu scene
- IRefreshable interface
- SceneRefresh reference-update system for persistent objects
- SceneLoadManager
### Changed
- Swapped Singleton pattern for Dependency Injection referencing pattern
- Made common objects persistent across scene loads (GameStateManager, UpdateManager, UiManager, BeatManager)
- Reorganized namespaces
### Removed
- Order of executions system for Update Manager

## [0.0.14] - 2022-10-20
### Added
- Input Playback System
- jumpSquat and dashWindup bools to InputState (improved / fixed input playback accuracy)
- Frame-by-frame mode for input playback system
### Changed
- GameplayComponents now use FixedUpdate
### Removed
- ReferenceManager singleton
### Fixed
- IUpdatable to avoid unnecessary calls to unused update functions
- Updatable registration issue

## [0.0.13] - 2022-14-09
### Added
- Placeholder tilemap
- Unity tilemap system (rule tiles)
- Placeholder character sprites
- Character animation triggers
- Dynamic bounds finder function for cam manager
- Camera player-follow function
- Camera zoom function (using camera bounds)
- GameplayReferenceManager to centralize config references
- tooltips to all fields in movement configs
- Test level
### Changed
- Turned common gameobjects into prefabs
- Moved config references to GameplayReferenceManager

## [0.0.12] - 2022-09-09
### Added
- Jump trigger with Beat event
- Basic fast fall
- Camera Manager
- Camera bounds tool
- Json save and load systems for Camera Bounds data
### Removed
- rise air drift point
- Graph visualizer (not useful anymore)
### Fixed
- Rise function

## [0.0.11] - 2022-09-06
### Added
- added basic music / beat system with accurate looping, beat events, and optional metronome
### Fixed
- improved movement configs

## [0.0.10] - 2022-08-17
### Added
- added scenes menu with basic load scene functionality
- created IUpdatable OrderOfExecutions json
- added text mesh pro and figured out IUpdatable execution order (still need to move it out of player prefs and into its own json)
- implemented custom order of operations for IUpdatables
### Removed
- removed ordered updatable types debugging objects
### Fixed
- fixed grounded diagonal dash (still some buggy behavior sometimes after touching right wall)
- fixed run turn bug (messy, needs refactor)

## [0.0.9] - 2022-08-12
### Added
- added debug mode (for now: free/direct position manipulation, no gravity). Activate by pressing Backspace
### Fixed
- fixed grounded up-dash and changed state after dash to fall
- fixed run-direction-change bug (was sometimes not turning around properly)
- Fixed dash

## [0.0.8] - 2022-08-11
### Added
- implemented very basic dash
- set up basic animator and anticipation states for jump and dash
### Changed
- optimized collision detection (boxcast instead of triggers)
### Fixed
- small cleanup

## [0.0.7] - 2022-08-09
### Added
- added keyboard binding for wall cling
### Changed
- refactored entire codebase (incl. new naming conventions)
### Fixed
- fixed wall cling and wall slide
- fixed occasional reference loss error in CharacterStatusEditor
- fixed CharacterStatusEditor script

## [0.0.6] - 2022-08-03
### Added
- Backed up character PNGs
- Added Dpad to Input System
- implemented basic wall cling timer
- implemented sliding run
- applied new air drift and drag system to Rise() and Fall()
### Changed
- clamped wall cling timer incrementation

## [0.0.5] - 2022-08-02
### Added
- implemented surface drag
- implemented wall jump angled rise
### Removed
- removed air drag and drift from Rise and fixed wall jump
### Fixed
- fixed crouched jump curve speed
- fixed wall slide collision detection
- fixed digital input

## [0.0.4] - 2022-08-01
### Added
- set up input deadzone and set up input system for gamepads
- implemented input system for keyboard and gc controller
- added input system package
- implemented basic gc controller support
- implemented walled state check facing orientation check
### Changed
- tweaked deadzone value
### Fixed
- fixed ride and fall state changes, air drift application, air drag application incl. increased and reduced drag

## [0.0.3] - 2022-07-31
### Added
- added directional and button input tracking to CharacterStatusEditor
- implemented wall slide drag
- implemented landing drag
- implemented air drag
- basic run and fall movement implemented
### Fixed
- fixed landing and air drift

## [0.0.2] - 2022-07-27
### Added
- implemented collision checks
- started implementing character status editor script
- started implementing new input => state machine => movement logic

## [0.0.1] - 2022-07-19
### Added
- created character states flags
- started initial systems setup
- Added placeholder Sound Barrier sprites
- IUpdatable implementation
- Created Unity Project