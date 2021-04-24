using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    internal class DynamicConsumer {
        private MqttClient _mqttClient;
        private string _mqttClientGuid = Guid.NewGuid().ToString();

        private ClientDevice _clientDevice;
        private List<ClientPropertyBase> _properties = new List<ClientPropertyBase>();

        public DynamicConsumer() { }

        public void Initialize(string mqttBrokerIpAddress, string[] topicDump) {
            // Initializing broker.
            _mqttClient = new MqttClient(mqttBrokerIpAddress);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
            _mqttClient.Connect(_mqttClientGuid);

            var homieTree = HomieTopicTreeParser.Parse(topicDump, DeviceFactory.BaseTopic);

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
            _clientDevice.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value), qosLevel, isRetained);
            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 1 });
            });
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            _clientDevice.HandlePublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message));
        }
    }
}
