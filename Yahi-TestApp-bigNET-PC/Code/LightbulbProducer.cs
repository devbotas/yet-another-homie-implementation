using System.Diagnostics;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace TestApp {
    internal class LightbulbProducer {
        private ResilientHomieBroker _broker = new ResilientHomieBroker();

        private HostDevice _hostDevice;
        private HostChoiceProperty _onOffSwitch;
        private HostColorProperty _color;
        private HostIntegerProperty _intensity;

        public LightbulbProducer() { }

        public void Initialize(string mqttBrokerIpAddress) {
            _hostDevice = DeviceFactory.CreateHostDevice("lightbulb", "Colorful lightbulb");

            #region General node

            _hostDevice.UpdateNodeInfo("general", "General information and properties", "no-type");

            // I think properties are pretty much self-explanatory in this producer.
            _color = _hostDevice.CreateHostColorProperty(PropertyType.Parameter, "general", "color", "Color", ColorFormat.Rgb);
            _color.PropertyChanged += (sender, e) => {
                Debug.Print($"Color changed to {_color.Value.ToRgbString()}");
            };
            _onOffSwitch = _hostDevice.CreateHostChoiceProperty(PropertyType.Parameter, "general", "is-on", "Is on", new[] { "OFF,ON" });
            _onOffSwitch.PropertyChanged += (sender, e) => {
                // Simulating some lamp behaviour.
                if (_onOffSwitch.Value == "ON") {
                    _intensity.Value = 50;
                }
                else {
                    _intensity.Value = 0;
                }
            };

            _intensity = _hostDevice.CreateHostIntegerProperty(PropertyType.Parameter, "general", "intensity", "Intensity", 0, "%");

            #endregion

            _broker.PublishReceived += _hostDevice.HandlePublishReceived;
            _broker.Initialize(mqttBrokerIpAddress, _hostDevice.WillTopic, _hostDevice.WillPayload);
            _hostDevice.Initialize(_broker.PublishToTopic, _broker.SubscribeToTopic);
        }
    }
}
