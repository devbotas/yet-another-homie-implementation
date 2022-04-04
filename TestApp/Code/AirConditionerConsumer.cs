using DevBot9.Protocols.Homie;

namespace TestApp;

internal class AirConditionerConsumer {
    private readonly NLog.ILogger _log = NLog.LogManager.GetCurrentClassLogger();

    private ClientDevice _clientDevice;
    private ClientNumberProperty _inletTemperature;
    private ClientChoiceProperty _turnOnOfProperty;
    private ClientChoiceProperty _actualState;

    public AirConditionerConsumer() { }

    public void Initialize(IClientDeviceConnection brokerConnection) {
        // Creating a air conditioner device.
        _clientDevice = DeviceFactory.CreateClientDevice("air-conditioner");

        // Creating properties.          
        _turnOnOfProperty = _clientDevice.CreateClientChoiceProperty(new ClientPropertyMetadata { PropertyType = PropertyType.Command, NodeId = "general", PropertyId = "turn-on-off", Format = "ON,OFF" });
        _actualState = _clientDevice.CreateClientChoiceProperty(new ClientPropertyMetadata { PropertyType = PropertyType.State, NodeId = "general", PropertyId = "actual-state", Format = "ON,OFF,STARTING", InitialValue = "OFF" });
        _actualState.PropertyChanged += (sender, e) => {
            _log.Info($"{_clientDevice.DeviceId}: property {_actualState.PropertyId} changed to {_actualState.Value}.");
        };

        _inletTemperature = _clientDevice.CreateClientNumberProperty(new ClientPropertyMetadata { PropertyType = PropertyType.State, NodeId = "general", PropertyId = "actual-air-temperature", DataType = DataType.Float, InitialValue = "0" });
        _inletTemperature.PropertyChanged += (sender, e) => {
            // Simulating some overheated dude.
            if (_inletTemperature.Value > 25) {
                _log.Info($"{_clientDevice.Name}: getting hot in here, huh?.. Let's try turning air conditioner on.");
                if (_actualState.Value != "ON") {
                    _turnOnOfProperty.Value = "ON";
                }
            }
        };

        // Initializing all the Homie stuff.
        _clientDevice.Initialize(brokerConnection);
    }
}
