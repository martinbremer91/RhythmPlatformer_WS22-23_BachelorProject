# Changelog - Rhythm Platformer (working title)

## Format

## ## [0.0.1] - 2022-08-25
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

## [Unreleased] (git.log)

## [0.0.10] - 2022-08-17
- added scenes menu with basic load scene functionality
- fixed grounded diagonal dash (still some buggy behavior sometimes after touching right wall)
- removed ordered updatable types debugging objects
- created IUpdatable OrderOfExecutions json
- added tmp and figured out IUpdatable execution order (still need to move it out of player prefs and into its own json)
- implemented custom order of operations for IUpdatables
- fixed run turn bug (messy, needs refactor)

## [0.0.9] - 2022-08-12
- changed frame rate back to uncapped
- fixed grounded up-dash and changed state after dash to fall
- fixed run-direction-change bug (was sometimes not turning around properly)
- Fixed dash
- added 60fps target frame rate
- added debug mode

## [0.0.8] - 2022-08-11
- optimized collision detection (boxcast instead of triggers)
- implemented very basic dash
- small cleanup
- set up basic animator and anticipation states for jump and dash

## [0.0.7] - 2022-08-09
- fixed wall cling and wall slide
- added keyboard binding for wall cling
- fixed occasional reference loss error in CharacterStatusEditor
- fixed CharacterStatusEditor script
- refactored entire codebase (incl. new naming conventions)
- started making wall cling changes

## [0.0.6] - 2022-08-03
- Backed Up CH PNGs
- Added Dpad to Input System
- clamped wall cling timer incrementation
- implemented basic wall cling timer
- implemented sliding run
- applied new air drift and drag system to Rise() and Fall()

## [0.0.5] - 2022-08-02
- fixed crouched jump curve speed
- fixed wall slide collision detection
- removed air drag and drift from Rise and fixed wall jump
- implemented surface drag
- fixed digital input
- implemented wall jump angled rise

## [0.0.4] - 2022-08-01
- tweaked deadzone value
- set up input deadzone and set up input system for gamepads
- implemented input system for keyboard and gc controller
- added input system package
- implemented basic gc controller support
- implemented walled state check facing orientation check
- fixed ride and fall state changes, air drift application, air drag application incl. increased and reduced drag

## [0.0.3] - 2022-07-31
- fixed landing and air drift
- added directional and button input tracking to CharacterStatusEditor
- implemented wall slide drag
- implemented landing drag
- implemented air drag
- basic run and fall movement implemented

## [0.0.2] - 2022-07-27
- made progress with collision state handling
- started implementing character status editor script
- implemented collision checks
- started implementing new input => state machine => movement logic

## [0.0.1] - 2022-07-19
- created character states flags
- fixed IUpdatable implementation
- Continued systems setup
- started initial systems setup
- Added placeholder Sound Barrier sprites
- Created Unity Project
- Initial commit