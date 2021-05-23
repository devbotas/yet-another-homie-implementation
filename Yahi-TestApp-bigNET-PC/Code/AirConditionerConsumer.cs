using System;
using System.Diagnostics;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace TestApp {
    internal class AirConditionerConsumer {
        private ResilientHomieBroker _broker = new ResilientHomieBroker();

        private ClientDevice _clientDevice;
        private ClientFloatProperty _inletTemperature;
        private ClientEnumProperty _turnOnOfProperty;
        private ClientEnumProperty _actualState;

        public AirConditionerConsumer() { }

        public void Initialize(string mqttBrokerIpAddress) {
            // Creating a air conditioner device.
            _clientDevice = DeviceFactory.CreateClientDevice("air-conditioner");

            // Creating properties.          
            _turnOnOfProperty = _clientDevice.CreateClientEnumProperty(new ClientPropertyMetadata { PropertyType = PropertyType.Command, NodeId = "general", PropertyId = "turn-on-off", Format = "ON,OFF" });
            _actualState = _clientDevice.CreateClientEnumProperty(new ClientPropertyMetadata { PropertyType = PropertyType.State, NodeId = "general", PropertyId = "actual-state", Format = "ON,OFF,STARTING" });
            _actualState.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Actual state: {_actualState.Value}");
            };

            _inletTemperature = _clientDevice.CreateClientFloatProperty(new ClientPropertyMetadata { PropertyType = PropertyType.State, NodeId = "general", PropertyId = "actual-air-temperature", DataType = DataType.Float, });
            _inletTemperature.PropertyChanged += (sender, e) => {
                // Simulating some overheated dude.
                if (_inletTemperature.Value > 25) {
                    Console.WriteLine("Getting hot in here, huh?.. Let's try turning air conditioner on.");
                    if (_actualState.Value != "ON") {
                        _turnOnOfProperty.Value = "ON";
                    }
                }
            };

            // Initializing all the Homie stuff.
            _broker.PublishReceived += _clientDevice.HandlePublishReceived;
            _broker.Initialize(mqttBrokerIpAddress);
            _clientDevice.Initialize(_broker.PublishToTopic, _broker.SubscribeToTopic);
        }
    }
}
