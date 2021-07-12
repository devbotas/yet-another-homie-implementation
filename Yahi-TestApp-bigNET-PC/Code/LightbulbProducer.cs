using System;
using System.Diagnostics;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace TestApp {
    internal class LightbulbProducer {
        private PahoHostDeviceConnection _broker = new PahoHostDeviceConnection();

        private HostDevice _hostDevice;
        private HostChoiceProperty _onOffSwitch;
        private HostColorProperty _color;
        private HostNumberProperty _intensity;

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
            _onOffSwitch = _hostDevice.CreateHostChoiceProperty(PropertyType.Parameter, "general", "is-on", "Is on", new[] { "OFF", "ON" }, "OFF");
            _onOffSwitch.PropertyChanged += (sender, e) => {
                // Simulating some lamp behaviour.
                if (_onOffSwitch.Value == "ON") {
                    _intensity.Value = 50;
                }
                else {
                    _intensity.Value = 0;
                }
            };

            _intensity = _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, "general", "intensity", "Intensity", 0, "%");

            #endregion

            _broker.Initialize(mqttBrokerIpAddress);
            _hostDevice.Initialize(_broker, (severity, message) => { Console.WriteLine($"{severity}:{message}"); });
        }
    }
}
