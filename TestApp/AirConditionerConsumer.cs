using System;
using System.Diagnostics;
using System.Text;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    internal class AirConditionerConsumer {
        private MqttClient _mqttClient;
        private string _mqttClientGuid = Guid.NewGuid().ToString();

        private ClientDevice _clientDevice;
        private ClientFloatProperty _inletTemperature;
        private ClientStringProperty _turnOnOfProperty;
        private ClientIntegerProperty _actualPower;
        private ClientStringProperty _actualState;

        public AirConditionerConsumer() { }

        public void Initialize(string mqttBrokerIpAddress) {
            // Initializing broker.
            _mqttClient = new MqttClient(mqttBrokerIpAddress);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
            _mqttClient.Connect(_mqttClientGuid);

            // Creating a air conditioner device.
            _clientDevice = DeviceFactory.CreateClientDevice("air-conditioner");

            // Creating properties.
            _turnOnOfProperty = _clientDevice.CreateClientStringProperty(PropertyType.Command, "general", "turn-on-off");
            _actualState = _clientDevice.CreateClientStringProperty(PropertyType.State, "general", "actual-state");
            _actualState.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Actual state: {_actualState.Value}");
            };

            _inletTemperature = _clientDevice.CreateClientFloatProperty(PropertyType.State, "general", "actual-air-temperature");
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
            _clientDevice.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value), qosLevel, isRetained);
            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 1 });
            });
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            _clientDevice.HandlePublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message));
        }
    }
}
