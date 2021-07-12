# Changelog

## [0.10.0-preview.30] - 2021-07-12
### Added
- Device base class is now disposable.
### Changed
- Restructuring TestApp initialization routine and logging more stuff for error detection.
### Fixed
- Initial value was not set for client properties when checking possible enum values.
- ClientDeviceMetadata will not not parse topics with empty payload.
- Fixed Lightbulb producer is-on property initialization.
- Exposing client property ID for read-only usage.


## [0.10.0-preview.20] - 2021-07-11
### Fixed
- Fixing node topic paths.


## [0.10.0-preview.18] - 2021-07-08
### Changed
- Reworked connection logic again. Client and Host sides now have different connection to broker types. May be easier to handle client-only applications which may now have a single connection for all client devices. Host devices are still one connection per device.
### Fixed
- "SET" messages are now sent as non-retained.


## [0.10.0-preview.13] - 2021-07-02
### Fixed
- Publishing Device state on reconnect to clear "lost" state.


## [0.10.0-preview.5] - 2021-06-29
### Added
- Adding IMqttBroker interface. Every real-world broker client now needs a wrapper implementing it. 

### Changed
- Blending in broker functionality into Device class to simplify initialization.
- Host/Client devices now require to be initialized with provided IMqttBroker instance.
- Renaming Float|String|Enum|String properties to Number|Text|Choice|Text. This better reflects real life usage.

### Removed
- Removing Boolean properties. Enum (now Choice) property is superior.
- Removing Integer properties. Float (now Number) property is superior.


## [0.9.0] - 2021-06-28
### Added
- Adding an ability to parse entire ClientDevice object from ClientDeviceMetadata. This should help a lot for generic consumers like BHD which shows all the properties of all devices.
- ClientDevice now also uses PropertyChanged events.
- Companion Homie.Utilities Nuget now has a ResilientHomieBroker class. It can be used to create a reconnecting Homie connection.
- Host Devices now expose Last Will topic and payload. Users can use those when connecting to broker.
- Implemented ClientColorProperty and ClientEnumProperty.
- Added alternative FetchDevices method to HomieTopicFetcher

### Changed
- DeviceId is now a public device property available for everyone to consume.
- HomieTopicTreeParser moved to a separate Homie.Utilities Nuget.
- Completely revamping HomieTopicTreeParser. It is now more robust and parses as much as possible and returns a perfectly valid Homie device tree.
- Client-side property creators now actually check if data is valid and throws exceptions if it is not.
- HomieColor is now based on static methods.
- Using "Fx" format for float properties instead of "0.0#".

### Fixed
- ClientDevice had incorrectly working topic and state hooks.
- Commands will now not be executed during initialization if there a retained topic in MQTT broker.
- Command properties now do not publish to main topic on startup.
- Moving TreeParser to proper namespace.
- Fixed naming for Lightbulb property so it reflects being a parameter.

### Removed
- Removing Percent data type because it is confusing and redundant.


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