using System;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;
using Tevux.Protocols.Mqtt;

namespace TestApp {
    internal class LightbulbConsumer {
        private NLog.ILogger _log = NLog.LogManager.GetCurrentClassLogger();
        private YahiTevuxClientConnection _broker = new();

        private ClientDevice _clientDevice;
        private ClientColorProperty _color;

        public LightbulbConsumer() { }

        public void Initialize(ChannelConnectionOptions channelOptions) {
            // Creating a air conditioner device.
            _clientDevice = DeviceFactory.CreateClientDevice("lightbulb");

            // Creating properties.          
            _color = _clientDevice.CreateClientColorProperty(new ClientPropertyMetadata { PropertyType = PropertyType.Parameter, NodeId = "general", PropertyId = "color", Format = "rgb", InitialValue = "0,0,0" });
            _color.PropertyChanged += (sender, e) => {
                if (_color.Value.RedValue > 0) {
                    Console.WriteLine("Me no like red!");
                    _color.Value = HomieColor.FromRgbString("0,128,128");
                }
            };

            // Initializing all the Homie stuff.
            _broker.Initialize(channelOptions);
            _clientDevice.Initialize(_broker);
        }
    }
}
