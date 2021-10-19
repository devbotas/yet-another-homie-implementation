using System.Diagnostics;
using DevBot9.Protocols.Homie;

namespace TestApp {
    internal class DynamicConsumer {
        private NLog.ILogger _log = NLog.LogManager.GetCurrentClassLogger();

        private ClientDevice _clientDevice;

        public DynamicConsumer() { }

        public void Initialize(IClientDeviceConnection brokerConnection, ClientDeviceMetadata deviceMetadata) {
            _clientDevice = DeviceFactory.CreateClientDevice(deviceMetadata);

            for (var i = 0; i < _clientDevice.Nodes.Length; i++) {
                Debug.Print($"Iterating over nodes. Currently: \"{_clientDevice.Nodes[i].Name}\" with {_clientDevice.Nodes[i].Properties.Length} properties.");

                foreach (var property in _clientDevice.Nodes[i].Properties) {
                    property.PropertyChanged += (sender, e) => {
                        Debug.WriteLine($"Value of property \"{property.Name}\" changed to \"{property.RawValue}\".");
                    };
                }
            }

            // Initializing all the Homie stuff.
            _clientDevice.Initialize(brokerConnection);
        }
    }
}
