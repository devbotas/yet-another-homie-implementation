using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;
using Tevux.Protocols.Mqtt;

namespace TestApp;

internal class LightbulbProducer {
    private readonly NLog.ILogger _log = NLog.LogManager.GetCurrentClassLogger();
    private readonly YahiTevuxHostConnection _broker = new();

    private HostDevice _hostDevice;
    private HostChoiceProperty _onOffSwitch;
    private HostColorProperty _color;
    private HostNumberProperty _intensity;

    public LightbulbProducer() { }

    public void Initialize(ChannelConnectionOptions channelOptions) {
        _hostDevice = DeviceFactory.CreateHostDevice("lightbulb", "Colorful lightbulb");

        #region General node

        _hostDevice.UpdateNodeInfo("general", "General information and properties", "no-type");

        // I think properties are pretty much self-explanatory in this producer.
        _color = _hostDevice.CreateHostColorProperty(PropertyType.Parameter, "general", "color", "Color", ColorFormat.Rgb);
        _color.PropertyChanged += (sender, e) => {
            _log.Info($"Color changed to {_color.Value.ToRgbString()}");
        };
        _onOffSwitch = _hostDevice.CreateHostChoiceProperty(PropertyType.Parameter, "general", "is-on", "Is on", new[] { "OFF", "ON" }, "OFF");
        _onOffSwitch.PropertyChanged += (sender, e) => {
            // Simulating some lamp behaviour.
            if (_onOffSwitch.Value == "ON") {
                _intensity.Value = 50;
            } else {
                _intensity.Value = 0;
            }
        };

        _intensity = _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, "general", "intensity", "Intensity", 0, "%");

        #endregion

        _broker.Initialize(channelOptions);
        _hostDevice.Initialize(_broker);
    }
}
