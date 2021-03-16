# Changelog

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