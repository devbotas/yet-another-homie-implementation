using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace TestApp {
    internal class DynamicConsumer {
        private ResilientHomieBroker _broker = new ResilientHomieBroker();

        private ClientDevice _clientDevice;
        private List<ClientPropertyBase> _properties = new List<ClientPropertyBase>();

        public DynamicConsumer() { }

        public void Initialize(string mqttBrokerIpAddress, string[] topicDump) {
            var homieTree = HomieTopicTreeParser.Parse(topicDump, DeviceFactory.BaseTopic, out var _);

            if (homieTree.Length == 0) { throw new ArgumentException("Please give me at least one device..."); }

            _clientDevice = DeviceFactory.CreateClientDevice(homieTree[0].Id);

            for (var i = 0; i < homieTree[0].Nodes.Length; i++) {
                Debug.Print($"Iterating over nodes. Currently: \"{homieTree[0].Nodes[i].Id}\" with {homieTree[0].Nodes[i].Properties.Length} properties.");

                foreach (var propertyMetadata in homieTree[0].Nodes[i].Properties) {
                    Debug.Print($"    Creating property \"{propertyMetadata.PropertyId}\" of type {propertyMetadata.DataType}. It is a {propertyMetadata.PropertyType}.");
                    if (propertyMetadata.DataType == DataType.String) {
                        var newProperty = _clientDevice.CreateClientStringProperty(propertyMetadata);
                        _properties.Add(newProperty);
                        newProperty.PropertyChanged += (sender, e) => {
                            Debug.WriteLine($"Value of property \"{newProperty.Name}\" changed to \"{newProperty.Value}\".");
                        };
                    }
                    if (propertyMetadata.DataType == DataType.Integer) {
                        var newProperty = _clientDevice.CreateClientIntegerProperty(propertyMetadata);
                        _properties.Add(newProperty);
                        newProperty.PropertyChanged += (sender, e) => {
                            Debug.WriteLine($"Value of property \"{newProperty.Name}\" changed to \"{newProperty.Value}\".");
                        };
                    }
                    if (propertyMetadata.DataType == DataType.Float) {
                        var newProperty = _clientDevice.CreateClientFloatProperty(propertyMetadata);
                        _properties.Add(newProperty);
                        newProperty.PropertyChanged += (sender, e) => {
                            Debug.WriteLine($"Value of property \"{newProperty.Name}\" changed to \"{newProperty.Value.ToString(CultureInfo.InvariantCulture)}\".");
                        };
                    }

                    if (propertyMetadata.DataType == DataType.Boolean) {
                        var newProperty = _clientDevice.CreateClientBooleanProperty(propertyMetadata);
                        _properties.Add(newProperty);
                        newProperty.PropertyChanged += (sender, e) => {
                            Debug.WriteLine($"Value of property \"{newProperty.Name}\" changed to \"{newProperty.Value}\".");
                        };
                    }
                }

            }

            // Initializing all the Homie stuff.
            _broker.PublishReceived += _clientDevice.HandlePublishReceived;
            _broker.Initialize(mqttBrokerIpAddress);
            _clientDevice.Initialize(_broker.PublishToTopic, _broker.SubscribeToTopic);
        }
    }
}
