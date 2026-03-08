# Changelog


## [3.1.7] - 2025-09-26
### Changed
- Updated core cavrnus libraries to receieve room tag data.
### Fixed
- Fixes to auto-referenced dll's. Specifically fixes ambiguous dependencies with packages like Meta.

## [3.1.6] - 2025-09-23
### Added
- Added XR interaction toolkit sample in separate registry package
- CavrnusAvatarFlag is now a base class and provides an AwaitUser now for safe access.
### Fixed
- Updated core libraries to fix issues with room tags

## [3.1.5] - 2025-09-16
### Fixed
- Fix Unity 6 compilation issues with IntegrationInfo type

## [3.1.4] - 2025-09-11
### Added
- Add a prompt UI and settings to use device-code based login at runtime.
### Changed
- Updated device code login flow success message.
- Assign server domain to csc when modifying auth method.
### Fixed
- Fixed app metrics posting, and other similar analytic updates.

## [3.1.3] - 2025-08-28
### Added
- Added new editor authentication quick setup. Users can now sign in or sign up for a cavrnus domain from editor.
### Fixed
- Fixed issues with Unity.Input assembly definition dependency as well as Editor assembly definition being compiled for all platforms
- Fixed cavrnus canvas prefab adding second event system if one already exists in scene.

## [3.1.2] - 2025-08-12
### Changed
- CollabPlugins folder renamed to Plugins
### Removed
- Removed Tests from distributed package to reduce size.
### Fixed
- Fixed package import errors regarding dll plugins

## [3.1.1] - 2025-07-23
### Added
- Added readmes for both the showcase and mobile demo samples.
### Fixed
- Fixed camera UI dropdowns for mobile devices
- Android and iOS RTC fixes

## [3.1.0] - 2025-07-16
### Added
- Added third party licenes
- Added new mobile demo for iOS and Android testing
### Fixed
- Fixed Android permissions not prompting in builds; voice/video works now.

## [3.0.12] - 2025-07-03
### Fixed
- Fixed ai prefab dependencies

## [3.0.11] - 2025-07-03
### Fixed
- Fixed missing AI script

## [3.0.10] - 2025-07-03
### Fixed
- Version fixery

## [3.0.4] - 2025-07-03
### Changed
- Some fixes to AI button

## [3.0.3] - 2025-07-02
### Fixed
- Fixed build issue caused by missing editor guards.
- Fixed README not updating version and other information for releases.

## [3.0.2] - 2025-07-02
### Added
- Added button to fetch journal in AI-readable format

## [3.0.1] - 2025-06-26
### Added
- Added URP tag to Cavrnus Sample Scene.
### Fixed
- Fixes to README, including links and updates to install instructions.
- Fixed the Property Creation Window validation logic as well as minor GUILayout issues.

## [3.0.0] - 2025-06-26
### Added
- Added new scriptable object based property binding system to simplify UI to actor data syncing.
- Added complete API documentation to static calls
- Added consistent namespaces, including assembly definitions to limit code pollution in projects.
- Added dialog upon new import of package prompting TextMeshPro installation.
### Changed
- Majority of assets, scripts, scenes have been renamed for consistency and to meet Unity standards.
- Primary sample scene now implements a few different methods of syncing and binding data, including the new scriptable objects method.
### Removed
- Many unused and duplicated assets have been removed, reducing both project size and complexity.
### Fixed
- Fixed RTC video sharing bug where having only one video device available prevented sharing from occuring.
- Removed consistent warnings and errors on initial import of package.

## [2.19.3] - 2025-01-02
### Fixed
- Fixes to underlying microphone detection logic to ensure valid mic is found and is polled for validity.

## [2.19.1] - 2024-12-12
### Added
- Added Analytics Gathering tools to track which objects users are looking at.

### Changed
- Rollback to older version of LiveSwitch

### Fixed
- Several UV texture fixes when streaming. Still known issue for Ios stream textures



## [2.19.0] - 2024-12-11
### Added
- Added Analytics Gathering tools to track which objects users are looking at.
- Add audio and video toggles to spatial connector component
- Updated RTC and added permissions helper for ios devices



## [2.18.0] - 2024-12-02
### Added
- Add BindSpaceInfo() binding



## [2.17.0] - 2024-12-02
### Added
- Added current CavrnusSpaceInfo to CavrnusSpaceConnection
- Added RequestRemoteUserMute() call to mute remote users
- Added ConnectionId to CavrnusUser

### Changed
- Removed None video option. Toggle button defaults to last used device on startup.

### Fixed
- Fixed missing dynamic localUser binds to handle proper rebinding.


## [2.16.0] - 2024-11-13
### Added
- Added RtcContext shutdown call when SpaceConnection is disposed of. Handles removal of generated gameObjects.
- Added FetchRemoteContentInfoById call to provide useful file metadata
- New ServerSelection menu shows when the CSC is not provided a server.

### Changed
- Updated metadata function names to be more general. Bind/Fetch calls no longer have "local" prefix.

### Fixed
- Fixed several login flows not saving needed information for api calls.



## [2.15.5] - 2024-10-29
### Changed
- "None" options in the Cavrnus Spatial Connector now say "Custom" to reduce confusion.


## [2.15.4] - 2024-10-21
### Fixed
- Fixed missing rebroadcast of Avatar position and visibility after changing spaces within the same tag.


## [2.15.2] - 2024-10-17
### Fixed
- Local user specific bindings will now remap when changing spaces.



## [2.15.1] - 2024-10-16
### Fixed
- Fix localUser bindings.
- Fix broken ui references in minimal ui
- Add default no audio/video rtc option again



## [2.15.0] - 2024-10-15
### Added
- Added IncludeRTC option in CavrnusSpaceJoinOptionsconfig object

### Changed
- RTC fetches/gets are now SpaceConnection specific. Calls are updated to use CavrnusSpaceConnection extension.

### Fixed
- Notify wasn't being setup for each auth path. Caused broken RTC for guests.



## [2.14.4] - 2024-10-14
### Fixed
- Fix sample prefabs dependencies



## [2.14.3] - 2024-10-14
### Added
- Added a more robust sample for multispace connections



## [2.14.2] - 2024-10-14
### Added
- Added status callbacks to metada calls
- Add Newtonsoft [com.unity.nuget.newtonsoft-json] 3.2.1 dependency



## [2.14.1] - 2024-10-13
### Added
- Added space swapping sample to package

### Fixed
- Fix JoinSpace backwards compatibility



## [2.14.0] - 2024-10-11
### Added
- Support for multi-space connections and rejoining spaces in a single session now added
- Added sample showing how to load images from URL as loader objects.
- Added extra avatar visibility options in CSC gameobject.

### Fixed
- Fixed various CSC gameobject login flows that were broken.



## [2.13.3] - 2024-09-30
### Fixed
- Fix missing refernce in ChatMenu



## [2.13.2] - 2024-09-30
### Fixed
- Fix namespaces that caused build errors



## [2.13.1] - 2024-09-26
### Added
- Editor XR helpers to simplify package and sample imports

### Fixed
- Add Editor defines to fix builds


## [2.13.0] - 2024-09-25
### Added
- Added new minimal UI
- Add latest CollabPlugins + Liveswitch updates
- Add avatar swap sample

### Changed
- Many fixes to UI scripts
- Reorganization of UI folder



## [2.12.6] - 2024-07-10
### Fixed
- Chat Menu Entries now show correct names/pics for users not in the space.



## [2.12.5] - 2024-07-09

### Fixed
- Better error messaging if the developer types in a bad Server/Join ID



## [2.12.4] - 2024-06-25
### Changed
- Renamed CavrnusUser UserId to UserAccountId for clarity

### Fixed
- ClientProvidedIntegrationInfo now checks for OSId null value. Caused spaces to hang when loading.


## [2.12.3] - 2024-06-24
### Changed
- Renamed CavrnusUser UserId to UserAccountId for clarity



## [2.12.2] - 2024-06-25
### Added
- Added a new feature

### Changed
- Formatting changes in README.md file



## [2.12.1] - 2024-06-24
### Added
- Added more robust Readme to that includes more info to getting started



## [2.12.0] - 2024-06-17
### Added
- Upgraded protocol to support journal caching



## [2.11.3] - 2024-06-14
### Changed
- Posted operations now show the new local value immediately



## [2.11.2] - 2024-06-13
### Added
- Added spawned object instance to CavrnusSpawnedObject struct



## [2.11.1] - 2024-06-12
### Fixed
- Fixed Auto Space JoinID


## [2.10.5] - 2024-05-30
### Changed
- SpaceInfo is now more responsive
- SpaceList filtered by LastVisited

### Fixed
- AvatarVis is now transient bool


## [2.11] - 2024-06-11
### Added
- Added smoothing options to Transform Props



## [2.10.4] - 2024-05-21
### Fixed
- Fixed Avatar vis. Only posts vis bool update now.



## [2.10.3] - 2024-05-15
### Added
- Added new version of device info to work with current SDK
- Chat UI integrated into XR rig
- Updated base plugins

### Fixed
- Object creation handler fix



## [2.10.2] - 2024-05-06
### Fixed
- Rebuilt base modules to include chat fixes



## [2.10.1] - 2024-05-04
### Fixed
- Fixed double auth issue when save token is enabled for either member or guest.



## [2.10.0] - 2024-05-02
### Added
- Various updates to sample scene to coincide with Unreal demo scene.
- Remote avatars automatically hidden at spawn until movement data is received.

### Fixed
- Fixed chat removal bug. Using the correct owner Id now.



## [2.9.0] - 2024-04-24
### Added
- Add updated core modules



## [2.8.1] - 2024-04-24
### Added
- Added support for chat via post and bind messages calls.

### Changed
- Cleaned up sample scene container names.



## [2.8.0] - 2024-04-17
### Added
- Added SyncMaterialColor script with direct reference to material.
- New version of CollabPlugins including RoomSystem & general object loading refactor.

### Removed
- Removed unneeded SyncMaterialInstance and SyncMaterialShared scripts.



## [2.7.3] - 2024-04-09
### Fixed
- Fixed missing icons and demo scene setup.



## [2.7.2] - 2024-04-09
### Fixed
- Fix Collaboration Samples pathing



## [2.7.1] - 2024-04-09
### Added
- Add more examples to main demo scene



## [2.7.0] - 2024-04-04
### Added
- Add server input into welcome modal.
- Add join config to CSC component.
- Add optional callback to SpawnObject which gives the actual ob when it arrives.
- Add spawnObject shortcut that takes a Transform.
- Add new demo scene "Sample Cavrnus Connected Space" that includes common sync samples.

### Changed
- Rename ValueSync components to match latest in Unreal

### Fixed
- Sample scene cleanup. Updated out of date components.
- Fix builds breaking. Add Unity scripting symbols around custom property drawers.
- Fix broken XR menus due to deprecated code.



## [2.6.3] - 2024-03-28
### Added
- Property drawer password field component to hide CSC member password entry



## [2.6.2] - 2024-03-22
### Fixed
- Added safeties when checking values from server and local..



## [2.6.1] - 2024-03-20
### Changed
- Updated Collab Plugins



## [2.6.0] - 2024-03-20
### Removed
- Removed XR content out of this core package to dedicated com.cavrnus.xr package.



## [2.5.0] - 2024-03-20
### Removed
- Removed Holo Loader from CSC package. Now lives in it's own package.



## [2.4.16] - 2024-03-19
### Fixed
- Fixed user transients accidentally finalizing, causing unexpected properties to show



## [2.4.15] - 2024-03-18
### Fixed
- Fixed README image paths



## [2.4.14] - 2024-03-18
### Added
- Changes to README



## [2.4.13] - 2024-03-15
### Added
- Added package registry



## [2.4.11] - 2024-03-11
### Fixed
- Fixed euler angle properties detecting changes when none are present



## [2.4.10] - 2024-03-07
### Changed
- User login flow to account for member and guest auth tokens

### Fixed
- Fixed missing icons in space picker menu.
- Space picker menu results are now sorted alphabetically.
- Space picker menu always shows available spaces.


## [2.4.9] - 2024-03-06
### Changed
- Changed several menus to be visually consistent rest of UI



## [2.4.8] - 2024-03-06
### Fixed
- Fixed space picker loading UI sticking around



## [2.4.7] - 2024-03-05
### Added
- Added a new feature

### Fixed
- Fixed problems when locally cached user token is invalid



## [2.4.6] - 2024-03-05
### Fixed
- Fixed Space Picker error



## [2.4.5] - 2024-03-05
### Fixed
- Fixed change detection epsilons for colors, floats, and vectors



## [2.4.4] - 2024-03-05
### Added
- Added sync components for material textures and sprites
- Added several new demo scenes for various methods of updating materials/textures



## [2.4.3] - 2024-03-05
### Fixed
- Fixed Ping-Ponging Property Values when multiple Transients conflict



## [2.4.2] - 2024-02-29
### Added
- SyncXrCameraTransform no-code component

### Changed
- Now have specific Sync transform components for Local and World.



## [2.4.1] - 2024-02-28
### Fixed
- User tokens are cleared properly if Save is not set



## [2.4.0] - 2024-02-28
### Added
- User Tokens will now automatically cache if told to

### Fixed
- Users post their position properly on join



## [2.3.2] - 2024-02-27
### Added
- SyncWorldTransform sync component



## [2.3.1] - 2024-02-27
### Fixed
- Fixed error where ui canvas is missing from CSC



## [2.3.0] - 2024-02-27
### Fixed
- Missing holo library sprites 



## [2.2.18] - 2024-02-27
### Changed
- Updated login flow options with new verbiage as well
- Updated documentation links



## [2.2.17] - 2024-02-23
### Added
- Reverted a few changes with avatar propertynames
- Fix local user setup helper to be capital Transform



## [2.2.16] - 2024-02-23
### Fixed
- Avatar nametag rotation and transform property names



## [2.2.15] - 2024-02-23
### Fixed
- Fixed Nametag default Property Name


## [2.2.14] - 2024-02-23
### Changed
- Changed the CSC defaults to be blank for customers to fill in



## [2.2.13] - 2024-02-23
### Changed
- Changed both package and github repo name
- Updated pathing references as well

### Fixed
- Including a few meta files in transfer that were missing



## [2.2.12] - 2024-02-23
### Fixed
- Fixed statically-defined user props being redefined/overridden



## [2.2.11] - 2024-02-22
### Fixed
- Fixed property flickering due to value bounce-back



## [2.2.10] - 2024-02-21
### Fixed
- Fixed minor Avatar Manager warnings



## [2.2.9] - 2024-02-21
### Fixed
- Further Avatar Spawn Improvements



## [2.2.8] - 2024-02-21
### Fixed
- Fixes to CSC remote avatar spawning
- Welcome modal verbiage changes
- User name tag works again



## [2.2.7] - 2024-02-21
### Fixed
- Fixed spawned objects popping from origin by delaying 3 frames to let props catch up



## [2.2.6] - 2024-02-20
### Fixed
- Increased Physics Lambda for Transforms



## [2.2.5] - 2024-02-19
### Added
- Add library search for holos



## [2.2.4] - 2024-02-17
### Fixed
- Added #if check for older Unity version without VisionOS



## [2.2.3] - 2024-02-17
### Fixed
- Fixed file data paths on Android/iOS/MacOS



## [2.2.2] - 2024-02-16
### Fixed
- Fixed Holo Sample Path



## [2.2.1] - 2024-02-16
### Fixed
- Local user transforms are the only send-only option



## [2.2.0] - 2024-02-16
### Added
- Finished Holo Loading & Samples



## [2.1.4] - 2024-02-15
### Fixed
- Fixed some pathing issues with editor windows


## [2.1.3] - 2024-02-14
### Added
- Add new welcome modal with updated graphics

### Fixed
- Fixes to scenes with broken prefabs and configurating core asset
- Fixes to avatar components



## [2.1.2] - 2024-02-13
### Added
- Added new temporary banner for welcome modal and fix pathing issues



## [2.1.1] - 2024-02-12
### Fixed
- Both local and remote video streams now update correctly



## [2.1.0] - 2024-02-09
### Changed
- CavrnusPropertiesContainer names are now absolute, and are automatically filled in and managed for users by default



## [2.0.1] - 2024-02-08
### Removed
- Remove bad using


## [2.0.0] - 2024-02-08
### Changed
- Refactored all functions into CavrnusFunctionLibrary
- Reworked namespaces to match the new structure


## [1.5.0] - 2024-01-29
### Added
- Cavrnus core now has messaging for optional canvases.

### Fixed
- Cleaned up Welcome Modal. Auto load toggle feature implemented.


## [1.4.0] - 2024-01-24
### Changed
-Project core folder reorganization along with misc code cleanup.


## [1.3.0] - 2024-01-24
### Added
- Major folder reorg. Moved core samples content into main sdk folder.


## [1.2.0] - 2024-01-23
### Added
- Cavrnus Core as the primary setup mechanism.


## [1.1.0] - 2024-01-05
### Added
- Add v1 streamboard functionality.
- Added local avatar components for copresence.


## [1.0.0] - 2024-01-03
### Added
- Add permissions demo showcasing restricting desired UI elements in a scene.
- Add smooth copresence along with other enhancements to avatar system.
- Demo assets are now URP compliant.
- Fix flipped video streams.
- Remove Magic Leap specific option in cav settings object.


## [0.2.0] - 2023-12-11
### Added
- Add Ar components to handle locally adjusting world origin of Ar tracked objects.


## [0.1.0] - 2023-12-01
### Added
- Initial package creation.
