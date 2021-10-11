# Changelog

## [0.11.0] - 2021-10-11
### Added
- Adding support for .NET Standard 2.0 and 2.1.
- NuGets now have Source Link configured.


## [0.10.0] - 2021-08-02
### Added
- PahoBroker that wraps good ol' MQTT Paho's client in a IMqttBroker interface.

### Changed
- Reworked connection logic again. Client and Host sides now have different connection to broker types. May be easier to handle client-only applications which may now have a single connection for all client devices. Host devices are still one connection per device.