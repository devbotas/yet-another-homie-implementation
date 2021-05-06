using System;
using System.Text;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    internal class LightbulbConsumer {
        private MqttClient _mqttClient;
        private string _mqttClientGuid = Guid.NewGuid().ToString();

        private ClientDevice _clientDevice;
        private ClientColorProperty _color;

        public LightbulbConsumer() { }

        public void Initialize(string mqttBrokerIpAddress) {
            // Initializing broker.
            _mqttClient = new MqttClient(mqttBrokerIpAddress);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
            _mqttClient.Connect(_mqttClientGuid);

            // Creating a air conditioner device.
            _clientDevice = DeviceFactory.CreateClientDevice("lightbulb");

            // Creating properties.          
            _color = _clientDevice.CreateClientColorProperty(new ClientPropertyMetadata { PropertyType = PropertyType.Parameter, NodeId = "general", PropertyId = "color", Format = "rgb" });
            _color.PropertyChanged += (sender, e) => {
                Console.WriteLine("naaah, changing it.");
                var newColor = new HomieColor();
                newColor.SetRgb("128,128,250");
                _color.Value = newColor;
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
