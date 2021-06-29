using System;
using System.Diagnostics;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace TestApp {
    internal class DynamicConsumer {
        private IMqttBroker _broker = new PahoBroker();

        private ClientDevice _clientDevice;

        public DynamicConsumer() { }

        public void Initialize(string mqttBrokerIpAddress, string[] topicDump) {
            var homieTree = HomieTopicTreeParser.Parse(topicDump, DeviceFactory.BaseTopic, out var _);

            if (homieTree.Length == 0) { throw new ArgumentException("Please give me at least one device..."); }

            _clientDevice = DeviceFactory.CreateClientDevice(homieTree[0]);

            for (var i = 0; i < _clientDevice.Nodes.Length; i++) {
                Debug.Print($"Iterating over nodes. Currently: \"{_clientDevice.Nodes[i].Name}\" with {_clientDevice.Nodes[i].Properties.Length} properties.");

                foreach (var property in _clientDevice.Nodes[i].Properties) {
                    property.PropertyChanged += (sender, e) => {
#warning Should I expose _rawValue property for read access?.. Otherwise I cannot access it as a  PropertyBase member...
                        // Debug.WriteLine($"Value of property \"{property.Name}\" changed to \"{property.Value}\".");
                    };

                }
            }

            // Initializing all the Homie stuff.
            _broker.Initialize(mqttBrokerIpAddress);
            _clientDevice.Initialize(_broker, (severity, message) => { Console.WriteLine($"{severity}:{message}"); });
        }
    }
}
