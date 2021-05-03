using System;
using System.Diagnostics;
using System.Text;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    internal class LightbulbProducer {
        private MqttClient _mqttClient;
        private readonly string _mqttClientGuid = Guid.NewGuid().ToString();

        private HostDevice _hostDevice;
        private HostBooleanProperty _onOffSwitch;
        private HostColorProperty _color;
        private HostIntegerProperty _intensity;

        public LightbulbProducer() { }

        public void Initialize(string mqttBrokerIpAddress) {
            _mqttClient = new MqttClient(mqttBrokerIpAddress);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
            _mqttClient.Connect(_mqttClientGuid);

            _hostDevice = DeviceFactory.CreateHostDevice("lightbulb", "Colorful lightbulb");

            #region General node

            _hostDevice.UpdateNodeInfo("general", "General information and properties", "no-type");

            // I think properties are pretty much self-explanatory in this producer.
            _color = _hostDevice.CreateHostColorProperty(PropertyType.Parameter, "general", "color", "Color", ColorFormat.Rgb);
            _color.PropertyChanged += (sender, e) => {
                Debug.Print($"Color changed to {_color.Value.ToRgbString()}");
            };
            _onOffSwitch = _hostDevice.CreateHostBooleanProperty(PropertyType.Parameter, "general", "is-on", "Is on");
            _onOffSwitch.PropertyChanged += (sender, e) => {
                // Simulating some lamp behaviour.
                if (_onOffSwitch.Value == true) {
                    _intensity.Value = 50;
                }
                else {
                    _intensity.Value = 0;
                }
            };

            _intensity = _hostDevice.CreateHostIntegerProperty(PropertyType.Parameter, "general", "intensity", "Intensity", 0, "%");

            #endregion

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
