﻿using System.Diagnostics;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;
using Tevux.Protocols.Mqtt;

namespace TestApp {
    internal class DynamicConsumer {
        private NLog.ILogger _log = NLog.LogManager.GetCurrentClassLogger();
        private YahiTevuxClientConnection _broker = new();

        private ClientDevice _clientDevice;

        public DynamicConsumer() { }

        public void Initialize(ChannelConnectionOptions channelOptions, ClientDeviceMetadata deviceMetadata) {
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
            _broker.Initialize(channelOptions);
            _clientDevice.Initialize(_broker);
        }
    }
}
