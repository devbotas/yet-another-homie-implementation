﻿using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;
using Tevux.Protocols.Mqtt;

namespace TestApp {
    internal class AirConditionerConsumer {
        private YahiTevuxClientConnection _broker = new();

        private ClientDevice _clientDevice;
        private ClientNumberProperty _inletTemperature;
        private ClientChoiceProperty _turnOnOfProperty;
        private ClientChoiceProperty _actualState;

        public AirConditionerConsumer() { }

        public void Initialize(ChannelConnectionOptions channelOptions, AddToLogDelegate addToLog) {
            // Creating a air conditioner device.
            _clientDevice = DeviceFactory.CreateClientDevice("air-conditioner");

            // Creating properties.          
            _turnOnOfProperty = _clientDevice.CreateClientChoiceProperty(new ClientPropertyMetadata { PropertyType = PropertyType.Command, NodeId = "general", PropertyId = "turn-on-off", Format = "ON,OFF" });
            _actualState = _clientDevice.CreateClientChoiceProperty(new ClientPropertyMetadata { PropertyType = PropertyType.State, NodeId = "general", PropertyId = "actual-state", Format = "ON,OFF,STARTING", InitialValue = "OFF" });
            _actualState.PropertyChanged += (sender, e) => {
                addToLog($"Info:", $"{_clientDevice.DeviceId}: property {_actualState.PropertyId} changed to {_actualState.Value}.");
            };

            _inletTemperature = _clientDevice.CreateClientNumberProperty(new ClientPropertyMetadata { PropertyType = PropertyType.State, NodeId = "general", PropertyId = "actual-air-temperature", DataType = DataType.Float, InitialValue = "0" });
            _inletTemperature.PropertyChanged += (sender, e) => {
                // Simulating some overheated dude.
                if (_inletTemperature.Value > 25) {
                    addToLog($"Info", $"{_clientDevice.Name}: getting hot in here, huh?.. Let's try turning air conditioner on.");
                    if (_actualState.Value != "ON") {
                        _turnOnOfProperty.Value = "ON";
                    }
                }
            };

            // Initializing all the Homie stuff.
            _broker.Initialize(channelOptions, (severity, message) => addToLog(severity, "Broker:" + message));
            _clientDevice.Initialize(_broker, (severity, message) => addToLog(severity, "Device:" + message));
        }
    }
}
