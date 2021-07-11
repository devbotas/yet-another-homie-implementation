# Changelog

## [0.10.0-preview.18] - 2021-07-08
### Changed
- Reworked connection logic again. Client and Host sides now have different connection to broker types. May be easier to handle client-only applications which may now have a single connection for all client devices. Host devices are still one connection per device.


## [0.10.0-preview.6] - 2021-06-30
### Added
- PahoBroker that wraps good ol' MQTT Paho's client in a IMqttBroker interface.