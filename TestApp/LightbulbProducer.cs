using System;
using System.Diagnostics;
using System.Text;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    internal class LightbulbProducer {
        private MqttClient _mqttClient;
        private string _mqttBrokerIp = "172.16.0.3";
        private string _mqttClientGuid = Guid.NewGuid().ToString();

        private HostDevice _hostDevice;
        private HostBooleanProperty _onOffSwitch;
        private HostColorProperty _color;
        private HostIntegerProperty _intensity;

        public LightbulbProducer() {
            _mqttClient = new MqttClient(_mqttBrokerIp);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
        }

        public void Initialize() {
            _mqttClient.Connect(_mqttClientGuid);

            _hostDevice = DeviceFactory.CreateHostDevice("lightbulb", "Colorful lightbulb");

            _hostDevice.UpdateNodeInfo("general", "General information and properties", "no-type");

            _color = _hostDevice.CreateHostColorProperty(PropertyType.Parameter, "general", "color", "Color", ColorFormat.Rgb);
            _color.PropertyChanged += (sender, e) => {
                Debug.Print($"Color changed to {_color.Value.ToRgbString()}");
            };
            _onOffSwitch = _hostDevice.CreateHostBooleanProperty(PropertyType.Command, "general", "turn-on-off", "Turn device on or off");
            _intensity = _hostDevice.CreateHostIntegerProperty(PropertyType.Parameter, "general", "intensity", "Intensity", "%");

            _hostDevice.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value), 1, true);

            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 1 });
            });
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            _hostDevice.HandlePublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message));
        }
    }
}
