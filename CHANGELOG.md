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
## [End of unreleased section]

## [0.0.24] - 2022-11-22
### Added
- companion follow logic (makes arc on the way to player character)
- particle systems to companion
- basic jump crystal logic
- companion idle animation
- slide-on mechanic when character is only partly grounded 
- basic HUD with a beat line and a beat display (in own canvas so it can have post processing applied to it)
- controller / keyboard menu navigation
- SaveInProgress message to corner of screen while saving data
- placeholder loading screen
- low pass filter sweep on pause and unpause
### Changed
- changed collision detection to use OverlapPoint instead of BoxCast
- dash now recharges upon death
- settings menu slider and toggles update to match saved prefs on startup
- companion's position now resets to checkpoint on death
### Fixed
- handled application focus change and application pause
- companion physics pause toggle
- dash animation bug (when beginning to fall during dash windup)

## [0.0.23] - 2022-11-18
### Added
- demo level with written instructions
- basic pulse material overrides logic (change color, etc)
- character can now change color depending on whether his dash is available
- generic color palettes logic (used for pulse material colors, etc. Includes HDR color field)
### Changed
- disabled jump button in build
- cannot become walled while fast falling and holding down anymore
### Removed
- movement snapshot logic
### Fixed
- pulse shader material(_MainTex missing)
- disabled and set controls to neutral while dead
- character no longer jumps when dead  

## [0.0.22] - 2022-11-17
### Added
- key-art sprite (for testing and reference)
- scaffold tileset
- hazards with movement routines
- player progress data and corresponding save and load system
- settings menu
### Changed
- movement routine setting doesn't rely on game objects in hierarchy anymore (now just handles)
- updated unity to 2021.3.13f1
### Removed
- input playback system
### Fixed
- added exit play mode safeguards to all async while loops
- tilesets reimported with nearest-neighbor anti-aliasing
- made movement routine destination detection more reliable (was overshooting target position)

## [0.0.21] - 2022-11-14
### Added
- camera manager assistant
- basic checkpoint logic
- death and respawn logic
- fade screen for scene transitions
- hazards (trigger death)
- dash-crystals (recharge dash)
- track info text in beat status tab
- new version of track "Nines"
- metronome only mode (for easier rhythm tests)
- (savable) sound preference settings
### Changed
- dash angles(less vertical)
### Removed
- look ahead camera logic
- jump squat logic 
### Fixed
- rise and fall drift bugs

## [0.0.20] - 2022-11-10
### Added
- Implemented count-in functionality for BeatManager
- simple graphics to beat manager count-in
- New track ('Nines' beat track) and track data
- One-way plaftorms
- Crouch-to-fall-through functionality for platforms
- Vertical 3-tile moving platform prefab (wall-cling possible)
### Changed
- Increased default cam max size
- Moved audio clip field to track data scriptable object
- Moved track data to bootstrap / dependency injector
### Fixed
- Player now sticks to moving platforms with right state and no jitter

## [0.0.19] - 2022-11-08
### Added
- Implemented IPhysicsPausable interface
- Implemented IAnimatorPausable interface
- Platform effector and Level physics layer to moving platform prefab
- New Dash sprites
### Changed
- Cam bounds data text asset reference now in bootstrap object
- Updated scene loader menu for new scenes
### Removed
- Level editor scene
### Fixed
- Buggy slow fall after wall cling timer runs out
- Bug where jump comes out like a dash

## [0.0.18] - 2022-11-04
### Added
- PingScenesFolder function to Scenes menu
- Camera look up and down functionality
- Implemented min cam size
- Updated Sprite Sheet 2 (added Dash, Slide, Look Up sprites)
- Shader Graph package
- Pulse shader asset
- Post Processing package
- Bloom effect (global)
- Adjustable color mask range property for pulse shader
- Level Template scene
### Changed
- Substituted center bounds for character position bounds in CameraManager
- Changed handling of straight-down grounded dash and walled dash
- Implemented canDash for player character (replenishes on jump)
- Moved Cem Configs to separate asset
- Wall cling trigger doesn't need velocity or input towards wall to wall cling
- Level scenes hierarchy structure (standardized)
### Fixed
- Ian's test level 01 (added collider + level manager)
- Camera Manager corner case issue
- Walled dash orientation
- Wall cling jittering
- Facing orientation logic

## [0.0.17] - 2022-11-03
### Added
- Air drift cancel (to change directions while airborne)
- Level Manager
- Movement routine system (waypoint follower)
- Waypoint generator system (for movement routines)
- Ian's test level
- Small grid / tilemap for smaller level design elements
- Placeholder small tile palette
- Max camera size
### Changed
- Added start delay to beat manager
- air drift speed and air drag
### Fixed
- Slide physics bug
- Wall slide orientation issue
- Ground detection bug

## [0.0.16] - 2022-11-01
### Added
- New frames to player character animations
- Character Spirte Sheet Draft 1
### Fixed
- Animation triggers logic
- Sprite sheet format in Sprites folder

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