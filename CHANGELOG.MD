# Changelog

## [0.9.0-preview.34] - 2021-05-14
### Added
- Host Devices now expose Last Will topic and payload. Users can use those when connecting to broker.

### Changed
- Completely revamping HomieTopicTreeParser. It is now more robust and parses as much as possible and returns a perfectly valid Homie device tree.


## [0.9.0-preview.28] - 2021-05-07
### Added
- Implemented ClientColorProperty and ClientEnumProperty.
- Added alternative FetchDevices method to HomieTopicFetcher

### Changed
- Client-side property creators now actually check if data is valid and throws exceptions if it is not.
- HomieColor is now based on static methods.

### Fixed
- Commands will now not be executed during initialization if there a retained topic in MQTT broker.

### Removed
- Removing Percent data type because it is confusing and redundant.


## [0.9.0-preview.13] - 2021-05-03
### Added

### Changed
- Using "Fx" format for float properties instead of "0.0#".

### Fixed
- Command properties now do not publish to main topic on startup.
- Moving TreeParser to proper namespace.
- Fixed naming for Lightbulb property so it reflects being a parameter.


## [0.8.0] - 2021-04-24
### Added
- Adding a few unit tests.
- Adding few helper classes to parse existing MQTT tree.
- YAHI now supports nanoFramework! A lot of cool internal stuff (like genenerics) had to be replaced with simplier constructs. Code looks way more 2005-ish now.
- Both bigNET and nanoNET nugets are now built using Nuke build system.

### Changed
- Recuperator producer and consumer is now replaced by AirConditioner counterparts.
- Client-side properties can now also be created by parsing existing MQTT tree. Constructors had to be changed.
- Codebase was split into shared code project and satellite bigNET/nanoNET projects.

### Fixed
- A few small client-side bugs were fixed.


## [0.7.0] - 2021-03-16
### Added
- YAHI is now multi-target NuGet, adding support for .NET Standard 2.0 and .NET 5.
- All the classes and properties are now at least somewhat documented.

### Changed
- A few small non-breaking changes here ant there.


## [0.6.0] - 2021-03-12
### Added
- Homie property creators now allow to pass an initial value, which will be published during initialization.

### Fixed
- Now actually fixing wrong decimal delimiters in float/string conversion.


## [0.5.1] - 2021-03-11
### Fixed
- Adding InvarianCulture when converting floats to/from string, so we always have "." as decimal delimiter.


## [0.5.0] - 2021-02-16
### Added
- Adding HostEnum property.
- Adding HostDateTime property.


## [0.4.0] - 2021-02-06
### Added
- Adding HostColor property.
- Creating AirConditioner, Lightbulb sample producers.
- Added support for Homie Nodes, because apparently Device cannot have direct Properties.

### Changed
- Made MQTT delegates more strict to enforce correct QoS and retainability.

### Fixed
- Fixed QoS levels of published messages.


## [0.3.0] - 2021-01-29
### Fixed
- Mostly cleanup.


## [0.2.0] - 2021-01-26
## Changed
- Reworked everything to work differently.

## [0.1.0] - 2021-01-12
### Added
- Implemented basic producer/consumer functionality.